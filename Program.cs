using AI_genda_API;
using Hangfire;

using HangfireBasicAuthenticationFilter;
using Serilog;
// HELLO
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDependencies(builder.Configuration);

builder.Host.UseSerilog((context, config) =>
{
    config.ReadFrom.Configuration(context.Configuration);
});


var app = builder.Build();


 app.MapOpenApi();

 app.UseSwaggerUI(opts => opts.SwaggerEndpoint("/openapi/v1.json", "Ai genda"));


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

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthorization();

//app.MapHub<ChatHub>("hubs/chat");

app.UseStaticFiles();

app.MapControllers();

app.Run();
