using NotificationService.Domain.ValueObjects;

namespace NotificationService.Domain.Events;

/// <summary>
/// Domain event raised when a notification is successfully sent.
/// </summary>
public record NotificationSentEvent(
    Guid NotificationId,
    Recipient Recipient,
    DateTime SentAt
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
