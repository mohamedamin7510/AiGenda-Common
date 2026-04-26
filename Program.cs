using AI_genda_API.Abstractions.Enums;
using AI_genda_API.Services.AppConnectionService;
using Hangfire;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDependencies(builder.Configuration);

builder.Host.UseSerilog((HostBuilderContext, LoggerConfiguration) =>
    LoggerConfiguration.ReadFrom.Configuration(HostBuilderContext.Configuration)
);

var app = builder.Build();

app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI();

app.UseHangfireDashboard("/jobs",
    new DashboardOptions()
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
    }
);

// Register recurrent sync jobs
var recurringJobManager = app.Services.GetRequiredService<IRecurringJobManager>();
recurringJobManager.AddOrUpdate<IAppConnectionService>(
    "sync-google-calendar-daily",
    service => service.SyncAllActiveConnectionsAsync(null, CancellationToken.None),
    Cron.Daily());

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthorization();

//app.MapHub<ChatHub>("hubs/chat");

app.UseStaticFiles();

app.MapControllers();

app.Run();
