using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniJiraAspire.Server.Models;

namespace MiniJiraAspire.Server.Data.Configurations;

public class EpicConfiguraiton : IEntityTypeConfiguration<Epic>
{
    public void Configure(EntityTypeBuilder<Epic> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(2000);

    }

}

