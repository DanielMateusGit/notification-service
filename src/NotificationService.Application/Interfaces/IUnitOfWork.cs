namespace NotificationService.Application.Interfaces;

/// <summary>
/// Port: Unit of Work pattern
/// Gestisce la transazione e il salvataggio delle modifiche
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
