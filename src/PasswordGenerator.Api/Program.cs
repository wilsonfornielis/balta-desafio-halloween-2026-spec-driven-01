using Microsoft.EntityFrameworkCore;
using PasswordGenerator.Api.Application;
using PasswordGenerator.Api.Endpoints;
using PasswordGenerator.Api.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddSingleton<IPasswordGenerator, PasswordGenerator.Api.Application.PasswordGenerator>();
builder.Services.AddScoped<PasswordService>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
}

// Habilitado em todos os ambientes (RF03), inclusive no container
app.MapOpenApi();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "Password Generator API v1");
    options.DocumentTitle = "Password Generator API";
});

app.MapPasswordEndpoints();

app.Run();

public partial class Program;
