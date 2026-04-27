using System.Reflection;
using System.Text;
using AI_genda_API.Abstractions.Filters;
using AI_genda_API.Authentication.Filters;
using AI_genda_API.Entities;
using AI_genda_API.Presistience;
using AI_genda_API.Services.AppConnectionService;
using AI_genda_API.Services.AppConnectionService.Connectors;
using AI_genda_API.Services.AuthService;
using AI_genda_API.Services.EmailService;
using AI_genda_API.Services.FocusSessionService;
using AI_genda_API.Services.NoteService;
using AI_genda_API.Services.ProfileService;
using AI_genda_API.Services.RoleService;
using AI_genda_API.Services.SpaceService;
using AI_genda_API.Services.SubTaskService;
using AI_genda_API.Services.TaskService;
using AI_genda_API.Services.WorkSpaceService;
using AI_genda_API.Api.Settings;
using FluentValidation;
using FluentValidation.AspNetCore;
using Hangfire;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;

namespace AI_genda_API;

public static class Dependenciynjections
{
    public static IServiceCollection AddDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        // direct adding 
        services.AddOpenApi();
        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "Enter your Bearer token",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT"
            });
            options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
            {
                {
                    new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                    {
                        Reference = new Microsoft.OpenApi.Models.OpenApiReference
                        {
                            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        services.AddControllers(options =>
        {
            options.Filters.Add<AiResponseWrapperFilter>();
        })
        .ConfigureApiBehaviorOptions(options =>
        {
            var originalFactory = options.InvalidModelStateResponseFactory;
            options.InvalidModelStateResponseFactory = context =>
            {
                if (context.HttpContext.Request.Path.Value?.StartsWith("/api/ai", StringComparison.OrdinalIgnoreCase) == true)
                {
                    var errors = context.ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    var message = errors.Count > 0 ? string.Join(" | ", errors) : "Validation failed.";

                    return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(new
                    {
                        status = "error",
                        message = message
                    });
                }
                return originalFactory(context);
            };
        });

        services.AddScoped<IAuthService, AuthServic>();
        services.AddScoped<IWorkSpaceService, WorkSpaceService>();
        services.AddScoped<ISpaceService, SpaceService>();
        services.AddScoped<ITaskService, TaskService>();
        services.AddScoped<IEmailSender, EmailService>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<ISubTaskService, SubTaskService>();
        services.AddScoped<INoteService, NoteService>();
        services.AddScoped<IFocusSessionService, FocusSessionService>();

        services.AddOptions<MailSettings>().BindConfiguration(nameof(MailSettings)).ValidateDataAnnotations().ValidateOnStart();

        // App Connection Services (Google Calendar integration)
        services.AddHttpClient();
        services.AddScoped<IAppConnectorFactory, AppConnectorFactory>();
        services.AddScoped<IAppConnectionService, AppConnectionService>();

        // AI Agent Token & Crypto Services (Secure Storage and Silent Refreshes)
        services.AddScoped<Services.TokenManagement.ITokenEncryptionService, Services.TokenManagement.TokenEncryptionService>();
        services.AddScoped<Services.TokenManagement.ITokenManagerService, Services.TokenManagement.TokenManagerService>();

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        // functions 
        services.AddCorsMethod(configuration);
        services.AddMapsterGlobalConfiguration(configuration);
        services.RegisterAppContext(configuration);
        services.RegisterIdentityServices();
        services.AddValidatorsFromAssemblyContaining<Program>().AddFluentValidationAutoValidation();
        services.AddAuthenticationServices(configuration);
        services.AddOptionclassBinding(configuration);
        services.AddJobs(configuration);

        return services;
    }

    private static void AddCorsMethod(this IServiceCollection services, IConfiguration config)
    {
        var AllowedOrigins = config.GetSection("AllowedOrigins").Get<string>();

        services.AddCors(
            opts =>
            {
                opts.AddDefaultPolicy(
                    builder =>
                    {
                        builder.WithOrigins(AllowedOrigins!)
                               .AllowAnyMethod()
                               .AllowAnyHeader();
                    });
            }
        );
    }

    private static void AddOptionclassBinding(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JWTOptions>()
            .BindConfiguration(JWTOptions.SectionName).ValidateDataAnnotations().ValidateOnStart();
    }

    private static void AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IJWTProvider, JWTProvider>();
        services.AddAuthorization();
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, WorkspacePermissionAuthorizationHandler>();

        var jwtSettings = configuration.GetSection(JWTOptions.SectionName).Get<JWTOptions>();

        services.AddAuthentication(opts =>
        {
            opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings!.SymmetricKey)),
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
            };
        });

        services.Configure<IdentityOptions>(Option =>
        {
            Option.SignIn.RequireConfirmedEmail = true;
            Option.User.RequireUniqueEmail = true;
            Option.Password.RequiredLength = 8;
        });
    }

    private static void AddMapsterGlobalConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var config = TypeAdapterConfig.GlobalSettings;
        config.Scan(Assembly.GetAssembly(typeof(Program))!);
        services.AddSingleton<IMapper>(new Mapper(config));
    }

    private static void RegisterIdentityServices(this IServiceCollection services)
    {
        services.AddIdentity<ExtendedUser, ApplicationRole>()
            .AddEntityFrameworkStores<Presistience.AppContext>().AddDefaultTokenProviders();
    }

    private static void RegisterAppContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connstring = configuration["ConnectionStrings:connectionstring"];
        services.AddDbContext<Presistience.AppContext>(
            e => e.UseSqlServer(connstring).ConfigureWarnings(w =>
             w.Ignore(RelationalEventId.PendingModelChangesWarning)));
    }

    private static IServiceCollection AddJobs(this IServiceCollection services, IConfiguration configuration)
    {
        // Add Hangfire services.
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(configuration.GetConnectionString("HangfireConnection")));

        // Add the processing server as IHostedService
        services.AddHangfireServer();

        // Add framework services.
        services.AddMvc();

        return services;
    }
}