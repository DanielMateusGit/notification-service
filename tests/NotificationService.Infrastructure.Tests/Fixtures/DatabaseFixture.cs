using Microsoft.EntityFrameworkCore;
using NotificationService.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace NotificationService.Infrastructure.Tests.Fixtures;

/// <summary>
/// Fixture condivisa per i test di integrazione.
/// Avvia un container PostgreSQL e applica le migrations.
/// </summary>
public class DatabaseFixture : IAsyncLifetime
{
    private PostgreSqlContainer _postgres = null!;

    public AppDbContext Context { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        // Avvia PostgreSQL in Docker
        _postgres = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("notification_test")
            .WithUsername("test")
            .WithPassword("test")
            .Build();

        await _postgres.StartAsync();

        // Crea DbContext con connection string del container
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;

        Context = new AppDbContext(options);

        // Applica migrations per creare le tabelle
        await Context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await Context.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    /// <summary>
    /// Pulisce tutte le tabelle per garantire isolamento tra test.
    /// </summary>
    public async Task CleanupAsync()
    {
        Context.DeliveryAttempts.RemoveRange(Context.DeliveryAttempts);
        Context.Notifications.RemoveRange(Context.Notifications);
        Context.Templates.RemoveRange(Context.Templates);
        await Context.SaveChangesAsync();
    }
}
