using Microsoft.EntityFrameworkCore;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;

namespace NotificationService.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementazione PostgreSQL di ITemplateRepository.
/// Adapter che traduce le operazioni di persistenza in chiamate EF Core.
/// </summary>
public class PostgresTemplateRepository : ITemplateRepository
{
    private readonly AppDbContext _context;

    public PostgresTemplateRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Template?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Templates.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<Template?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        // Normalizza il nome per la ricerca (case-insensitive)
        var normalizedName = name.Trim().ToLowerInvariant();

        return await _context.Templates
            .FirstOrDefaultAsync(t => t.Name == normalizedName, cancellationToken);
    }

    public async Task<IReadOnlyList<Template>> GetByChannelAsync(
        NotificationChannel channel,
        CancellationToken cancellationToken = default)
    {
        return await _context.Templates
            .Where(t => t.Channel == channel)
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Template>> GetActiveByChannelAsync(
        NotificationChannel channel,
        CancellationToken cancellationToken = default)
    {
        return await _context.Templates
            .Where(t => t.Channel == channel && t.IsActive)
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Template template, CancellationToken cancellationToken = default)
    {
        await _context.Templates.AddAsync(template, cancellationToken);
    }

    public Task UpdateAsync(Template template, CancellationToken cancellationToken = default)
    {
        _context.Templates.Update(template);
        return Task.CompletedTask;
    }
}
