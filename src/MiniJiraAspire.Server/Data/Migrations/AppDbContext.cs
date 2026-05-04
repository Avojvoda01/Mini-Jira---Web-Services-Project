using MiniJiraAspire.Server.Models;

namespace MiniJiraAspire.Server.Migrations;
using Microsoft.EntityFrameworkCore;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Epic> Epics => Set<Epic>();
<<<<<<< HEAD
    public DbSet<Project> Projects => Set<Project>();
=======
    public DbSet<Comment> Comments => Set<Comment>();
    //public DbSet<User> Users => Set<User>();
>>>>>>> main

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
