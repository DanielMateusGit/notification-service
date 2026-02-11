namespace NotificationService.Domain.Events;

/// <summary>
/// Marker interface for domain events.
/// A domain event represents something significant that happened in the domain.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// When the event occurred
    /// </summary>
    DateTime OccurredOn { get; }
}
