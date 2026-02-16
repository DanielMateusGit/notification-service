using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;

namespace NotificationService.Application.Interfaces;

/// <summary>
/// Port: Repository per le notifiche
/// Definisce il contratto - l'implementazione sta in Infrastructure
/// </summary>
public interface INotificationRepository
{
    Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Notification>> GetByStatusAsync(NotificationStatus status, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Notification>> GetPendingAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Notification notification, CancellationToken cancellationToken = default);
    Task UpdateAsync(Notification notification, CancellationToken cancellationToken = default);
}
