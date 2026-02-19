using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationService.Domain.Entities;

namespace NotificationService.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configurazione EF Core per l'entity Template.
/// </summary>
public class TemplateConfiguration : IEntityTypeConfiguration<Template>
{
    public void Configure(EntityTypeBuilder<Template> builder)
    {
        // Tabella
        builder.ToTable("templates");

        // Primary Key
        builder.HasKey(t => t.Id);

        // Properties
        builder.Property(t => t.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(t => t.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.Channel)
            .HasColumnName("channel")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(t => t.Subject)
            .HasColumnName("subject")
            .HasMaxLength(500);

        builder.Property(t => t.Body)
            .HasColumnName("body")
            .IsRequired();

        builder.Property(t => t.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at");

        // Indici
        builder.HasIndex(t => t.Name)
            .IsUnique()
            .HasDatabaseName("ix_templates_name");

        builder.HasIndex(t => t.Channel)
            .HasDatabaseName("ix_templates_channel");

        builder.HasIndex(t => new { t.Channel, t.IsActive })
            .HasDatabaseName("ix_templates_channel_is_active");
    }
}
