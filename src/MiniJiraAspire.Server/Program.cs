using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using MiniJiraAspire.Server.Endpoints.Admin;
using MiniJiraAspire.Server.Endpoints.Auth;
using MiniJiraAspire.Server.Endpoints.Comments;
using MiniJiraAspire.Server.Endpoints.Epics;
using MiniJiraAspire.Server.Endpoints.Tasks;
using MiniJiraAspire.Server.Migrations;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;
using MiniJiraAspire.Server.Services.Auth;
using Scalar.AspNetCore;
using Microsoft.EntityFrameworkCore;
using MiniJiraAspire.Server.Endpoints.Projects;
using MiniJiraAspire.Server.Endpoints.Users;

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
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtIssuer = jwtSection["Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer is not configured.");
var jwtAudience = jwtSection["Audience"] ?? throw new InvalidOperationException("Jwt:Audience is not configured.");
var jwtSecret = jwtSection["Secret"] ?? throw new InvalidOperationException("Jwt:Secret is not configured.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });
builder.Services.AddAuthorization();

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

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();

    app.MapScalarApiReference();


}

await DbSeeder.MigrateAndSeedAsync(app.Services);

// Auth
app.MapAuthEndpoints();

// Tasks
app.MapTaskEndpoints();
app.MapTaskActionEndpoints();

// Comments
app.MapCommentEndpoints();

// Epics
app.MapEpicEndpoints();

// Projects
app.MapProjectEndpoints();

// Users
app.MapUserEndpoints();

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

// Required for WebApplicationFactory<Program> in integration tests
public partial class Program;
