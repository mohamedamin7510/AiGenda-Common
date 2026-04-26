var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDependencies(builder.Configuration);

builder.Host.UseSerilog((HostBuilderContext, LoggerConfiguration) =>
    LoggerConfiguration.ReadFrom.Configuration(HostBuilderContext.Configuration)
);

var app = builder.Build();

app.UseExceptionHandler();

app.MapOpenApi();

app.UseSwaggerUI(opts => opts.SwaggerEndpoint("/openapi/v1.json", "AiGenda"));

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
    });

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.UseStaticFiles();
app.MapControllers();

app.Run();
