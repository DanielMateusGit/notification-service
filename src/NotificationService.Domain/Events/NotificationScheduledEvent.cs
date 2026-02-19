using NotificationService.Domain.ValueObjects;

namespace NotificationService.Domain.Events;

/// <summary>
/// Domain event raised when a notification is scheduled for future delivery.
/// </summary>
public record NotificationScheduledEvent(
    Guid NotificationId,
    Recipient Recipient,
    DateTime ScheduledAt
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
