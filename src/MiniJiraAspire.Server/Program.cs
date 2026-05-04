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
using MiniJiraAspire.Server.Features.Project.GetAllProjectsQuery;
using MiniJiraAspire.Server.Features.Project.GetProjectByIdQuery;
using MiniJiraAspire.Server.Features.Project.CreateProjectCommand;
using MiniJiraAspire.Server.Features.Project.UpdateProjectCommand;
using MiniJiraAspire.Server.Features.Project.DeleteProjectCommand;

DotNetEnv.Env.Load();
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

builder.Services.AddScoped<IEpicRepository, EpicRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IGetAllProjectsQuery, GetAllProjectsQuery>();
builder.Services.AddScoped<IGetProjectByIdQuery, GetProjectByIdQuery>();
builder.Services.AddScoped<ICreateProjectCommand, CreateProjectCommand>();
builder.Services.AddScoped<IUpdateProjectCommand, UpdateProjectCommand>();
builder.Services.AddScoped<IDeleteProjectCommand, DeleteProjectCommand>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();

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
