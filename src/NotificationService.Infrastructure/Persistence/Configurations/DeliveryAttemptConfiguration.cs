using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationService.Domain.Entities;

namespace NotificationService.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configurazione EF Core per l'entity DeliveryAttempt.
/// </summary>
public class DeliveryAttemptConfiguration : IEntityTypeConfiguration<DeliveryAttempt>
{
    public void Configure(EntityTypeBuilder<DeliveryAttempt> builder)
    {
        // Tabella
        builder.ToTable("delivery_attempts");

        // Primary Key
        builder.HasKey(da => da.Id);

        // Properties
        builder.Property(da => da.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(da => da.NotificationId)
            .HasColumnName("notification_id")
            .IsRequired();

        builder.Property(da => da.AttemptNumber)
            .HasColumnName("attempt_number")
            .IsRequired();

        builder.Property(da => da.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(da => da.ErrorMessage)
            .HasColumnName("error_message")
            .HasMaxLength(2000);

        builder.Property(da => da.AttemptedAt)
            .HasColumnName("attempted_at")
            .IsRequired();

        builder.Property(da => da.CompletedAt)
            .HasColumnName("completed_at");

        // Indici
        builder.HasIndex(da => da.NotificationId)
            .HasDatabaseName("ix_delivery_attempts_notification_id");

        builder.HasIndex(da => new { da.NotificationId, da.AttemptNumber })
            .IsUnique()
            .HasDatabaseName("ix_delivery_attempts_notification_attempt");
    }
}
