using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;

namespace NotificationService.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configurazione EF Core per l'entity Notification.
/// Definisce mapping tabella, colonne, indici e relazioni.
/// </summary>
public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        // Tabella
        builder.ToTable("notifications");

        // Primary Key
        builder.HasKey(n => n.Id);

        // Properties
        builder.Property(n => n.Id)
            .HasColumnName("id")
            .ValueGeneratedNever(); // L'ID è generato dal Domain

        builder.Property(n => n.Recipient)
            .HasColumnName("recipient")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(n => n.Channel)
            .HasColumnName("channel")
            .HasConversion<string>() // Salva come stringa nel DB
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(n => n.Content)
            .HasColumnName("content")
            .IsRequired();

        builder.Property(n => n.Subject)
            .HasColumnName("subject")
            .HasMaxLength(500);

        builder.Property(n => n.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(n => n.Priority)
            .HasColumnName("priority")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(n => n.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(n => n.ScheduledAt)
            .HasColumnName("scheduled_at");

        builder.Property(n => n.SentAt)
            .HasColumnName("sent_at");

        builder.Property(n => n.FailedAt)
            .HasColumnName("failed_at");

        builder.Property(n => n.ErrorMessage)
            .HasColumnName("error_message")
            .HasMaxLength(2000);

        // Ignora i Domain Events (non vanno persistiti)
        builder.Ignore(n => n.DomainEvents);

        // Indici per query frequenti
        builder.HasIndex(n => n.Status)
            .HasDatabaseName("ix_notifications_status");

        builder.HasIndex(n => n.ScheduledAt)
            .HasDatabaseName("ix_notifications_scheduled_at");

        builder.HasIndex(n => new { n.Status, n.ScheduledAt })
            .HasDatabaseName("ix_notifications_status_scheduled_at");

        // Relazione con DeliveryAttempts
        builder.HasMany<DeliveryAttempt>()
            .WithOne()
            .HasForeignKey(da => da.NotificationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
