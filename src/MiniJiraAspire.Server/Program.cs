using Microsoft.Extensions.Hosting.Admin.Roles;
using Microsoft.Extensions.Hosting.Admin.Users;
using Microsoft.Extensions.Hosting.Auth.Login;
using Microsoft.Extensions.Hosting.Auth.Register;
using Microsoft.Extensions.Hosting.Comments;
using Microsoft.Extensions.Hosting.Epics;
using Microsoft.Extensions.Hosting.Tasks;
using Microsoft.Extensions.Hosting.Tasks.Actions;
using MiniJiraAspire.Server.Migrations;
using MiniJiraAspire.Server.Persistence.Repositories;
using MiniJiraAspire.Server.Persistence.Repositories.Interfaces;
using Scalar.AspNetCore;
using Microsoft.EntityFrameworkCore;
using MiniJiraAspire.Server.Endpoints.Projects;

DotNetEnv.Env.Load();
var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

// CORS policy for development - allows the frontend running on localhost:5173 to access the API.
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(options =>
        options.AddPolicy("FrontendDev", policy =>
            policy.WithOrigins("http://localhost:5173")
                .AllowAnyHeader()
                .AllowAnyMethod()));
}

// AddDbContext registers AppDbContext as scoped, which is the right lifetime for one HTTP request.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IEpicRepository, EpicRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

// Enable CORS for development environment
if (app.Environment.IsDevelopment())
{
    app.UseCors("FrontendDev");
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();

    app.MapScalarApiReference();


}

await DbSeeder.MigrateAndSeedAsync(app.Services);

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
