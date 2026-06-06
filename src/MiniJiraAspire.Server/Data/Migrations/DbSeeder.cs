using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MiniJiraAspire.Server.Migrations;
using MiniJiraAspire.Server.Models;

namespace MiniJiraAspire.Server.Data.Migrations;

public class DbSeeder
{
    public static async Task MigrateAndSeedAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();

        if (db.Database.IsRelational())
            await db.Database.MigrateAsync();
        else
            await db.Database.EnsureCreatedAsync();

        // Seed users first so they can be referenced as project owners.
        var sampleUsers = new[]
        {
            new User { Email = "alice.alpha@mini-jira.local", PasswordHash = "", DisplayName = "Alice Alpha", Role = UserRole.Admin },
            new User { Email = "bob.alpha@mini-jira.local",   PasswordHash = "", DisplayName = "Bob Alpha",   Role = UserRole.User },
            new User { Email = "carol.beta@mini-jira.local",  PasswordHash = "", DisplayName = "Carol Beta",  Role = UserRole.User },
            new User { Email = "dave.beta@mini-jira.local",   PasswordHash = "", DisplayName = "Dave Beta",   Role = UserRole.User },
        };

        foreach (var u in sampleUsers)
        {
            var existing = await db.Users.FirstOrDefaultAsync(x => x.Email == u.Email);
            if (existing is null)
            {
                u.PasswordHash = passwordHasher.HashPassword(u, "password123");
                db.Users.Add(u);
            }
            else if (string.IsNullOrEmpty(existing.PasswordHash))
            {
                existing.PasswordHash = passwordHasher.HashPassword(existing, "password123");
            }
        }

        await db.SaveChangesAsync();

        var alice = await db.Users.FirstOrDefaultAsync(u => u.Email == "alice.alpha@mini-jira.local");
        var bob   = await db.Users.FirstOrDefaultAsync(u => u.Email == "bob.alpha@mini-jira.local");
        var carol = await db.Users.FirstOrDefaultAsync(u => u.Email == "carol.beta@mini-jira.local");
        var dave  = await db.Users.FirstOrDefaultAsync(u => u.Email == "dave.beta@mini-jira.local");

        // Seed projects. Alice owns Alpha, Carol owns Beta.
        var projectAlpha = await db.Projects.FirstOrDefaultAsync(p => p.Name == "Project Alpha");
        if (projectAlpha is null)
        {
            projectAlpha = new Project
            {
                Name = "Project Alpha",
                Description = "Core platform development for the first product release.",
                CreatedById = alice?.Id,
            };
            db.Projects.Add(projectAlpha);
        }
        else if (projectAlpha.CreatedById is null && alice is not null)
        {
            projectAlpha.CreatedById = alice.Id;
        }

        var projectBeta = await db.Projects.FirstOrDefaultAsync(p => p.Name == "Project Beta");
        if (projectBeta is null)
        {
            projectBeta = new Project
            {
                Name = "Project Beta",
                Description = "Customer-facing portal and integrations.",
                CreatedById = carol?.Id,
            };
            db.Projects.Add(projectBeta);
        }
        else if (projectBeta.CreatedById is null && carol is not null)
        {
            projectBeta.CreatedById = carol.Id;
        }

        await db.SaveChangesAsync();

        // Seed epics — one per user, owner creates first, member creates second.
        var alphaEpicExists = await db.Epics.AnyAsync(e => e.ProjectId == projectAlpha.Id);
        if (!alphaEpicExists)
        {
            db.Epics.AddRange(
                new Epic
                {
                    Name = "Security",
                    Description = "Security related tickets regarding Authentication",
                    ProjectId = projectAlpha.Id,
                    CreatedById = alice?.Id,
                },
                new Epic
                {
                    Name = "Backend",
                    Description = "Tasks regarding Persistence and Business Logic",
                    ProjectId = projectAlpha.Id,
                    CreatedById = bob?.Id,
                });
        }
        else
        {
            // Patch existing epics that have no CreatedById.
            var alphaEpics = await db.Epics.Where(e => e.ProjectId == projectAlpha.Id).ToListAsync();
            foreach (var (epic, owner) in new[] { (alphaEpics.FirstOrDefault(e => e.Name == "Security"), alice), (alphaEpics.FirstOrDefault(e => e.Name == "Backend"), bob) })
            {
                if (epic is not null && epic.CreatedById is null && owner is not null)
                    epic.CreatedById = owner.Id;
            }
        }

        var betaEpicExists = await db.Epics.AnyAsync(e => e.ProjectId == projectBeta.Id);
        if (!betaEpicExists)
        {
            db.Epics.AddRange(
                new Epic
                {
                    Name = "Frontend",
                    Description = "All tasks regarding UI",
                    ProjectId = projectBeta.Id,
                    CreatedById = carol?.Id,
                },
                new Epic
                {
                    Name = "API Integration",
                    Description = "Third-party API integrations for the portal",
                    ProjectId = projectBeta.Id,
                    CreatedById = dave?.Id,
                });
        }
        else
        {
            var betaEpics = await db.Epics.Where(e => e.ProjectId == projectBeta.Id).ToListAsync();
            foreach (var (epic, owner) in new[] { (betaEpics.FirstOrDefault(e => e.Name == "Frontend"), carol), (betaEpics.FirstOrDefault(e => e.Name == "API Integration"), dave) })
            {
                if (epic is not null && epic.CreatedById is null && owner is not null)
                    epic.CreatedById = owner.Id;
            }
        }

        await db.SaveChangesAsync();

        // Seed project memberships.
        var desiredMemberships = new[]
        {
            new { ProjectId = projectAlpha.Id, UserId = bob?.Id },
            new { ProjectId = projectBeta.Id,  UserId = dave?.Id },
        };

        foreach (var m in desiredMemberships)
        {
            if (m.UserId is null) continue;

            var exists = await db.ProjectMembers.AnyAsync(pm => pm.ProjectId == m.ProjectId && pm.UserId == m.UserId.Value);
            if (!exists)
                db.ProjectMembers.Add(new ProjectMember { ProjectId = m.ProjectId, UserId = m.UserId.Value });
        }

        await db.SaveChangesAsync();

        // Seed tasks — 4 per project with varied statuses, priorities and creators.
        var alphaTasksExist = await db.TaskItems.AnyAsync(t => t.ProjectId == projectAlpha.Id);
        if (!alphaTasksExist)
        {
            var securityEpic = await db.Epics.FirstOrDefaultAsync(e => e.ProjectId == projectAlpha.Id && e.Name == "Security");
            var backendEpic  = await db.Epics.FirstOrDefaultAsync(e => e.ProjectId == projectAlpha.Id && e.Name == "Backend");

            db.TaskItems.AddRange(
                new TaskItem
                {
                    Title = "Set up JWT authentication",
                    Description = "Implement token-based auth using JwtBearer middleware.",
                    Status = "In Progress",
                    Priority = "High",
                    ProjectId = projectAlpha.Id,
                    EpicId = securityEpic?.Id,
                    CreatedById = alice?.Id,
                    AssigneeId = alice?.Id,
                },
                new TaskItem
                {
                    Title = "Add password hashing",
                    Description = "Use BCrypt or ASP.NET Identity hasher for stored passwords.",
                    Status = "Done",
                    Priority = "High",
                    ProjectId = projectAlpha.Id,
                    EpicId = securityEpic?.Id,
                    CreatedById = alice?.Id,
                    AssigneeId = bob?.Id,
                },
                new TaskItem
                {
                    Title = "Design task repository interface",
                    Description = "Define ITaskRepository with CRUD and filter methods.",
                    Status = "Open",
                    Priority = "Medium",
                    ProjectId = projectAlpha.Id,
                    EpicId = backendEpic?.Id,
                    CreatedById = bob?.Id,
                    AssigneeId = bob?.Id,
                },
                new TaskItem
                {
                    Title = "Implement EF Core migrations",
                    Description = "Set up initial schema migrations and seeder.",
                    Status = "Review",
                    Priority = "Medium",
                    ProjectId = projectAlpha.Id,
                    EpicId = backendEpic?.Id,
                    CreatedById = bob?.Id,
                    AssigneeId = alice?.Id,
                });
        }

        var betaTasksExist = await db.TaskItems.AnyAsync(t => t.ProjectId == projectBeta.Id);
        if (!betaTasksExist)
        {
            var frontendEpic = await db.Epics.FirstOrDefaultAsync(e => e.ProjectId == projectBeta.Id && e.Name == "Frontend");
            var apiEpic      = await db.Epics.FirstOrDefaultAsync(e => e.ProjectId == projectBeta.Id && e.Name == "API Integration");

            db.TaskItems.AddRange(
                new TaskItem
                {
                    Title = "Build project dashboard UI",
                    Description = "Create the main dashboard with task metrics and charts.",
                    Status = "In Progress",
                    Priority = "High",
                    ProjectId = projectBeta.Id,
                    EpicId = frontendEpic?.Id,
                    CreatedById = carol?.Id,
                    AssigneeId = carol?.Id,
                },
                new TaskItem
                {
                    Title = "Implement dark mode toggle",
                    Description = "Support system preference and manual override.",
                    Status = "Open",
                    Priority = "Low",
                    ProjectId = projectBeta.Id,
                    EpicId = frontendEpic?.Id,
                    CreatedById = carol?.Id,
                    AssigneeId = dave?.Id,
                },
                new TaskItem
                {
                    Title = "Integrate Stripe payment API",
                    Description = "Connect Stripe SDK for subscription billing flows.",
                    Status = "Open",
                    Priority = "High",
                    ProjectId = projectBeta.Id,
                    EpicId = apiEpic?.Id,
                    CreatedById = dave?.Id,
                    AssigneeId = dave?.Id,
                },
                new TaskItem
                {
                    Title = "Map third-party webhook events",
                    Description = "Handle incoming events and update internal state accordingly.",
                    Status = "Review",
                    Priority = "Medium",
                    ProjectId = projectBeta.Id,
                    EpicId = apiEpic?.Id,
                    CreatedById = dave?.Id,
                    AssigneeId = carol?.Id,
                });
        }

        await db.SaveChangesAsync();
    }
}
