using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;

namespace NotificationService.Application.Interfaces;

/// <summary>
/// Port: Repository per i template di notifica
/// Definisce il contratto - l'implementazione sta in Infrastructure
/// </summary>
public interface ITemplateRepository
{
    Task<Template?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Template?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Template>> GetByChannelAsync(NotificationChannel channel, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Template>> GetActiveByChannelAsync(NotificationChannel channel, CancellationToken cancellationToken = default);
    Task AddAsync(Template template, CancellationToken cancellationToken = default);
    Task UpdateAsync(Template template, CancellationToken cancellationToken = default);
}
