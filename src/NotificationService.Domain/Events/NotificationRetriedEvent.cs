using NotificationService.Domain.ValueObjects;

namespace NotificationService.Domain.Events;

/// <summary>
/// Domain event raised when a failed notification is retried.
/// </summary>
public record NotificationRetriedEvent(
    Guid NotificationId,
    Recipient Recipient,
    string? PreviousError
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
