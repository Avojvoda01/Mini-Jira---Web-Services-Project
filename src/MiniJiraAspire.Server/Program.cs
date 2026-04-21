using Microsoft.Extensions.Hosting.Admin.Roles;
using Microsoft.Extensions.Hosting.Admin.Users;
using Microsoft.Extensions.Hosting.Auth.Login;
using Microsoft.Extensions.Hosting.Auth.Register;
using Microsoft.Extensions.Hosting.Comments;
using Microsoft.Extensions.Hosting.Epics;
using Microsoft.Extensions.Hosting.Projects;
using Microsoft.Extensions.Hosting.Tasks;
using Microsoft.Extensions.Hosting.Tasks.Actions;
using MiniJiraAspire.Server.Migrations;
using Scalar.AspNetCore;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

// AddDbContext registers AppDbContext as scoped, which is the right lifetime for one HTTP request.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();

    app.MapScalarApiReference();


}

// Auth
app.MapLogin();
app.MapRegister();

// Tasks
app.MapTaskEndpoints();
app.MapTaskActionEndpoints();

// Comments
app.MapCommentEndpoints();

// Epics
app.MapEpicEndpoints();

// Projects
app.MapProjectEndpoints();

// Admin
app.MapAdminUserEndpoints();
app.MapAdminRoleEndpoints();

app.MapDefaultEndpoints();

app.UseFileServer();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}