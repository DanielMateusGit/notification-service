using NotificationService.Application.Interfaces;

namespace NotificationService.Infrastructure.Persistence;

/// <summary>
/// Implementazione di IUnitOfWork.
/// Gestisce la transazione e il salvataggio delle modifiche.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // TODO: Prima di SaveChanges, dispatcha i Domain Events
        // Questo sarà implementato in Week 5-6 con MediatR
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
