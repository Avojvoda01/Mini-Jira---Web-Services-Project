using Microsoft.EntityFrameworkCore;
using MiniJiraAspire.Server.Models;

namespace MiniJiraAspire.Server.Migrations;

public class DbSeeder
{
    public static async Task MigrateAndSeedAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Using migrations keeps the day 2 sample aligned with the EF Core workflow we teach.
        await db.Database.MigrateAsync();

        if (await db.Projects.AnyAsync())
        {
            return;
        }

        var projectAlpha = new Project
        {
            Name = "Project Alpha",
            Description = "Core platform development for the first product release.",
        };

        var projectBeta = new Project
        {
            Name = "Project Beta",
            Description = "Customer-facing portal and integrations.",
        };

        db.Projects.AddRange(projectAlpha, projectBeta);
        await db.SaveChangesAsync();

        var epics = new[]
        {
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
            },
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
            },
        };

        db.Epics.AddRange(epics);
        await db.SaveChangesAsync();
    }
}