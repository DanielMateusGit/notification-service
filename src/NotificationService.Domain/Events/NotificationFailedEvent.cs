using NotificationService.Domain.Enums;

namespace NotificationService.Domain.Events;

/// <summary>
/// Domain event raised when a notification fails to be delivered.
/// </summary>
public record NotificationFailedEvent(
    Guid NotificationId,
    string Recipient,
    NotificationChannel Channel,
    string ErrorMessage,
    DateTime FailedAt
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
