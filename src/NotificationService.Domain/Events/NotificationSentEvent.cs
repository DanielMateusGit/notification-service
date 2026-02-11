using NotificationService.Domain.Enums;

namespace NotificationService.Domain.Events;

/// <summary>
/// Domain event raised when a notification is successfully sent.
/// </summary>
public record NotificationSentEvent(
    Guid NotificationId,
    string Recipient,
    NotificationChannel Channel,
    DateTime SentAt
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
