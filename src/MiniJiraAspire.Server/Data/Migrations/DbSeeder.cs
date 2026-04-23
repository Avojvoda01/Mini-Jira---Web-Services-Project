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

        if (await db.Epics.AnyAsync())
        {
            return;
        }
        
        var events = new[]
        {
            new Epic
            {
                Name = "Security",
                Description = "Security related tickets regarding Authentication",
            },
            new Epic
            {
                Name = "Frontend",
                Description = "All tasks regarding UI",
            },
            new Epic
            {
                Name = "Backend",
                Description = "Tasks regarding Persistence and Business Logic",
            },
                
        
        };

        db.Epics.AddRange(events);
        await db.SaveChangesAsync();
    }
}