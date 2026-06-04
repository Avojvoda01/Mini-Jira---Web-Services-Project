using System;
using System.Threading.Tasks;
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

        // Using migrations keeps the day 2 sample aligned with the EF Core workflow we teach.
        if (db.Database.IsRelational())
            await db.Database.MigrateAsync();
        else
            await db.Database.EnsureCreatedAsync();

        var projectAlpha = await db.Projects.FirstOrDefaultAsync(project => project.Name == "Project Alpha");
        if (projectAlpha is null)
        {
            projectAlpha = new Project
            {
                Name = "Project Alpha",
                Description = "Core platform development for the first product release.",
            };

            db.Projects.Add(projectAlpha);
        }

        var projectBeta = await db.Projects.FirstOrDefaultAsync(project => project.Name == "Project Beta");
        if (projectBeta is null)
        {
            projectBeta = new Project
            {
                Name = "Project Beta",
                Description = "Customer-facing portal and integrations.",
            };

            db.Projects.Add(projectBeta);
        }

        await db.SaveChangesAsync();

        var alphaEpicExists = await db.Epics.AnyAsync(epic => epic.ProjectId == projectAlpha.Id);
        var betaEpicExists = await db.Epics.AnyAsync(epic => epic.ProjectId == projectBeta.Id);

        if (!alphaEpicExists)
        {
            db.Epics.AddRange(
                new Epic
                {
                    Name = "Security",
                    Description = "Security related tickets regarding Authentication",
                    ProjectId = projectAlpha.Id,
                },
                new Epic
                {
                    Name = "Backend",
                    Description = "Tasks regarding Persistence and Business Logic",
                    ProjectId = projectAlpha.Id,
                });
        }

        if (!betaEpicExists)
        {
            db.Epics.AddRange(
                new Epic
                {
                    Name = "Frontend",
                    Description = "All tasks regarding UI",
                    ProjectId = projectBeta.Id,
                },
                new Epic
                {
                    Name = "API Integration",
                    Description = "Third-party API integrations for the portal",
                    ProjectId = projectBeta.Id,
                });
        }

        await db.SaveChangesAsync();
        await db.SaveChangesAsync();

        // Seed sample users (only add if missing). Keep style consistent with the
        // surrounding seeding logic and do not alter existing project/epic seeding.
        var sampleUsers = new[]
        {
            new User { Email = "alice.alpha@mini-jira.local", PasswordHash = "password123", DisplayName = "Alice Alpha", Role = UserRole.Admin },
            new User { Email = "bob.alpha@mini-jira.local", PasswordHash = "password123", DisplayName = "Bob Alpha", Role = UserRole.ProjectMember },
            new User { Email = "carol.beta@mini-jira.local", PasswordHash = "password123", DisplayName = "Carol Beta", Role = UserRole.ProjectMember },
            new User { Email = "dave.beta@mini-jira.local", PasswordHash = "password123", DisplayName = "Dave Beta", Role = UserRole.ProjectMember },
        };

        foreach (var u in sampleUsers)
        {
            var exists = await db.Users.AnyAsync(x => x.Email == u.Email);
            if (!exists)
            {
                db.Users.Add(u);
            }
        }

        await db.SaveChangesAsync();

        // Seed project memberships for the sample users. Only add missing links.
        var alice = await db.Users.FirstOrDefaultAsync(user => user.Email == "alice.alpha@mini-jira.local");
        var bob = await db.Users.FirstOrDefaultAsync(user => user.Email == "bob.alpha@mini-jira.local");
        var carol = await db.Users.FirstOrDefaultAsync(user => user.Email == "carol.beta@mini-jira.local");
        var dave = await db.Users.FirstOrDefaultAsync(user => user.Email == "dave.beta@mini-jira.local");

        var desiredMemberships = new[]
        {
            new { ProjectId = projectAlpha.Id, UserId = alice?.Id },
            new { ProjectId = projectAlpha.Id, UserId = bob?.Id },
            new { ProjectId = projectBeta.Id, UserId = carol?.Id },
            new { ProjectId = projectBeta.Id, UserId = dave?.Id },
        };

        foreach (var m in desiredMemberships)
        {
            if (m.UserId is null)
            {
                continue;
            }

            var exists = await db.ProjectMembers.AnyAsync(pm => pm.ProjectId == m.ProjectId && pm.UserId == m.UserId.Value);
            if (!exists)
            {
                db.ProjectMembers.Add(new ProjectMember { ProjectId = m.ProjectId, UserId = m.UserId.Value });
            }
        }

        await db.SaveChangesAsync();
    }
}