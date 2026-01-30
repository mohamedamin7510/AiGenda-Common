
using AI_genda_API.Services.AuthService;
using AI_genda_API.Services.FolderService;

namespace AI_genda_API;

public static class DependenciesInjection
{

    public static IServiceCollection AddDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        //short Services 
        services.AddOpenApi()
              .AddControllers();
        services.AddScoped<IAuthService, AuthServic>();
        services.AddCorsMethod(configuration);
        services.AddScoped<IFolderService , FolderService>();

        // tall services 
        services.AddMapsterGlobalConfiguration(configuration);
        services.RegisterAppContext(configuration);
        services.RegisterIdentityServices();
        services.AddValidatorsFromAssemblyContaining<Program>().AddFluentValidationAutoValidation();
        services.AddAuthenticationServices(configuration);
        services.AddOptionclassBinding(configuration);

        return services;
    }

    private static void AddCorsMethod(this IServiceCollection services , IConfiguration config )
    {
        var AllowedOrigins = config.GetSection("AllowedOrigins").Get<string[]>();

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
         var jwtSettings = configuration.GetSection(JWTOptions.SectionName).Get<JWTOptions>();
        services.AddAuthentication(opts => {
            opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme; }
        ).AddJwtBearer(options =>
        {
            options.SaveToken= true;
            options.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings!.SymmetricKey)),
                ValidIssuer =  jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,

            };

        });
    }

    private static void AddMapsterGlobalConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var config = TypeAdapterConfig.GlobalSettings;
        config.Scan(Assembly.GetAssembly(typeof(Program))!);

        services.AddSingleton<IMapper>(new Mapper(config));


    }
    private static void RegisterIdentityServices (this IServiceCollection services)
    {
        services.AddIdentity<ExtendedUser , IdentityRole>().AddEntityFrameworkStores<AppContext>(); 
    }

    private static void RegisterAppContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connstring = configuration["ConnectionStrings:connectionstring"];
        services.AddDbContext<AppContext>(e => e.UseSqlServer(connstring));
    }



}
