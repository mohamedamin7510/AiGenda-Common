using AI_genda_API;
using Hangfire;
using HangfireBasicAuthenticationFilter;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDependencies(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(opts => opts.SwaggerEndpoint("/openapi/v1.json", "Ai genda"));
}

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

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.Run();
