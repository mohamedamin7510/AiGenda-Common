using AI_genda_API;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDependencies(builder.Configuration);



var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(opts => opts.SwaggerEndpoint("/openapi/v1.json", "Ai genda"));
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.Run();
