using Microsoft.EntityFrameworkCore;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;

namespace NotificationService.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementazione PostgreSQL di INotificationRepository.
/// Adapter che traduce le operazioni di persistenza in chiamate EF Core.
/// </summary>
public class PostgresNotificationRepository : INotificationRepository
{
    private readonly AppDbContext _context;

    public PostgresNotificationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Notifications.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IReadOnlyList<Notification>> GetByStatusAsync(
        NotificationStatus status,
        CancellationToken cancellationToken = default)
    {
        return await _context.Notifications
            .Where(n => n.Status == status)
            .OrderBy(n => n.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Notification>> GetPendingAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        return await _context.Notifications
            .Where(n => n.Status == NotificationStatus.Pending)
            .Where(n => n.ScheduledAt == null || n.ScheduledAt <= now)
            .OrderBy(n => n.ScheduledAt ?? n.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        await _context.Notifications.AddAsync(notification, cancellationToken);
    }

    public Task UpdateAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        // EF Core Change Tracking: l'entity è già tracciata,
        // le modifiche saranno salvate con SaveChangesAsync()
        _context.Notifications.Update(notification);
        return Task.CompletedTask;
    }
}
