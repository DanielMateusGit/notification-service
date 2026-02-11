using NotificationService.Domain.Enums;

namespace NotificationService.Domain.Events;

/// <summary>
/// Domain event raised when a notification is scheduled for future delivery.
/// </summary>
public record NotificationScheduledEvent(
    Guid NotificationId,
    string Recipient,
    NotificationChannel Channel,
    DateTime ScheduledAt
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
