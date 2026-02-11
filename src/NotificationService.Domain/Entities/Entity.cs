using NotificationService.Domain.Events;

namespace NotificationService.Domain.Entities;

/// <summary>
/// Base class for all domain entities.
/// Provides domain events support.
/// </summary>
public abstract class Entity
{
    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>
    /// Domain events raised by this entity.
    /// Events are collected here and dispatched after SaveChanges.
    /// </summary>
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Adds a domain event to be dispatched later.
    /// Called by entity methods when something significant happens.
    /// </summary>
    /// <param name="domainEvent">The event to add</param>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Clears all domain events.
    /// Called by infrastructure after dispatching events.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
