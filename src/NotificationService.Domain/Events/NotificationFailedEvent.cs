using NotificationService.Domain.ValueObjects;

namespace NotificationService.Domain.Events;

/// <summary>
/// Domain event raised when a notification fails to be delivered.
/// </summary>
public record NotificationFailedEvent(
    Guid NotificationId,
    Recipient Recipient,
    string ErrorMessage,
    DateTime FailedAt
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
