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

        // Seed epics.
        var alphaEpicExists = await db.Epics.AnyAsync(e => e.ProjectId == projectAlpha.Id);
        var betaEpicExists  = await db.Epics.AnyAsync(e => e.ProjectId == projectBeta.Id);

        if (!alphaEpicExists)
        {
            db.Epics.AddRange(
                new Epic { Name = "Security",  Description = "Security related tickets regarding Authentication",  ProjectId = projectAlpha.Id },
                new Epic { Name = "Backend",   Description = "Tasks regarding Persistence and Business Logic",     ProjectId = projectAlpha.Id });
        }

        if (!betaEpicExists)
        {
            db.Epics.AddRange(
                new Epic { Name = "Frontend",        Description = "All tasks regarding UI",                          ProjectId = projectBeta.Id },
                new Epic { Name = "API Integration", Description = "Third-party API integrations for the portal",     ProjectId = projectBeta.Id });
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
    }
}
