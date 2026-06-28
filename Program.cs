using AI_genda_API.Abstractions.Enums;
using AI_genda_API.Services.AppConnectionService;
using Hangfire;
using Microsoft.AspNetCore.DataProtection;
using Serilog;
using System.IO;

try
{
    var builder = WebApplication.CreateBuilder(args);

    // الحل القياسي والرسمي الخالي من العواقب لبيئة MonsterASP
    // بنثبت مسار الحفظ في مجلد مادي App_Data/Keys لتوحيد المفاتيح عبر كل الـ Processes
    // وبنسيب دوت نت يتعامل مع الملفات بشكل طبيعي وبدون كلاسات كاستم مجهولة الهوية للـ IIS
    var keysFolder = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "Keys");
    builder.Services.AddDataProtection()
        .SetApplicationName("AiGenda")
        .PersistKeysToFileSystem(new DirectoryInfo(keysFolder));

    builder.Services.AddDependencies(builder.Configuration);

    builder.Host.UseSerilog((HostBuilderContext, LoggerConfiguration) =>
        LoggerConfiguration.ReadFrom.Configuration(HostBuilderContext.Configuration));

    var app = builder.Build();

    app.UseMiddleware<AI_genda_API.Middlewares.IntegrationExceptionHandlerMiddleware>();
    app.UseExceptionHandler("/api/error");

    var enableDiagnostics = app.Configuration.GetValue<bool>("EnableDiagnostics");
    if (enableDiagnostics)
    {
        app.MapOpenApi();
        app.UseSwagger();
        app.UseSwaggerUI(opts =>
        {
            opts.RoutePrefix = "swagger";
            opts.SwaggerEndpoint("/swagger/v1/swagger.json", "AiGenda");
        });

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

    try
    {
        var recurringJobManager = app.Services.GetRequiredService<IRecurringJobManager>();
        recurringJobManager.AddOrUpdate<IAppConnectionService>(
            "sync-google-calendar-daily",
            service => service.SyncAllActiveConnectionsAsync(null, CancellationToken.None),
            Cron.Daily());
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Hangfire recurring job registration failed.");
    }

    app.UseSerilogRequestLogging();
    app.UseHttpsRedirection();
    app.UseCors();

    app.UseAuthentication();
    app.UseAuthorization();

    app.UseStaticFiles();
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "startup_crash.txt");
    File.WriteAllText(logPath, $"[CRASH TIME: {DateTime.UtcNow}]\nException: {ex.Message}\nStackTrace: {ex.StackTrace}\nInner: {ex.InnerException?.Message}");
    throw;
}