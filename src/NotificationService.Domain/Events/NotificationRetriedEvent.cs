using NotificationService.Domain.Enums;

namespace NotificationService.Domain.Events;

/// <summary>
/// Domain event raised when a failed notification is retried.
/// </summary>
public record NotificationRetriedEvent(
    Guid NotificationId,
    string Recipient,
    NotificationChannel Channel,
    string? PreviousError
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
