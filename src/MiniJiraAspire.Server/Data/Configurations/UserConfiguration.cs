using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniJiraAspire.Server.Models;

namespace MiniJiraAspire.Server.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Persist the UserRole enum as its name (string) so the column stays human-readable
        // text and matches the enum-name wire format used by the API and JWT claims.
        builder.Property(user => user.Role)
            .HasConversion<string>();
    }
}
