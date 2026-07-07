using AI_genda_API.Abstractions.Enums;
using AI_genda_API.Services.AppConnectionService;
using Hangfire;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDependencies(builder.Configuration);

builder.Host.UseSerilog((HostBuilderContext, LoggerConfiguration) =>
    LoggerConfiguration.ReadFrom.Configuration(HostBuilderContext.Configuration));

var app = builder.Build();

app.UseMiddleware<AI_genda_API.Middlewares.IntegrationExceptionHandlerMiddleware>();
app.UseExceptionHandler("/api/error");

var enableDiagnostics = app.Configuration.GetValue<bool>("EnableDiagnostics");
if (enableDiagnostics)
{
    // Swagger & OpenAPI setup
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(opts =>
    {
        opts.RoutePrefix = "swagger";
        opts.SwaggerEndpoint("/swagger/v1/swagger.json", "AiGenda");
    });

    // Hangfire Dashboard setup
    app.UseHangfireDashboard("/jobs", new DashboardOptions()
    {
        Authorization =
        [
            new HangfireCustomBasicAuthenticationFilter()
            {
               User = app.Configuration.GetValue<string>("HangfireAuth:User"),
               Pass = app.Configuration.GetValue<string>("HangfireAuth:Pass")
            }
        ],
        DashboardTitle = "AiGenda-Jobs-DashBoard"
    });
}

// Register recurrent sync jobs
var recurringJobManager = app.Services.GetRequiredService<IRecurringJobManager>();
recurringJobManager.AddOrUpdate<IAppConnectionService>(
    "sync-google-calendar-daily",
    service => service.SyncAllActiveConnectionsAsync(null, CancellationToken.None),
    Cron.Daily());

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();
app.MapControllers();

app.Run();

public partial class Program { }