using Microsoft.EntityFrameworkCore;
using NotificationService.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace NotificationService.Infrastructure.Tests.Migrations;

/// <summary>
/// Test che verifica che le migrations funzionino correttamente.
/// </summary>
public class MigrationTests : IAsyncLifetime
{
    private PostgreSqlContainer _postgres = null!;
    private AppDbContext _context = null!;

    public async Task InitializeAsync()
    {
        _postgres = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .Build();

        await _postgres.StartAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;

        _context = new AppDbContext(options);
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    [Fact]
    public async Task Migrations_ApplySuccessfully()
    {
        // Act - Applica tutte le migrations
        await _context.Database.MigrateAsync();

        // Assert - Verifica che le tabelle esistano
        var tables = await _context.Database
            .SqlQuery<string>($"SELECT table_name FROM information_schema.tables WHERE table_schema = 'public'")
            .ToListAsync();

        Assert.Contains("notifications", tables);
        Assert.Contains("templates", tables);
        Assert.Contains("delivery_attempts", tables);
        Assert.Contains("__EFMigrationsHistory", tables);
    }

    [Fact]
    public async Task Migrations_CreateRecipientOwnedTypeColumns()
    {
        // Act
        await _context.Database.MigrateAsync();

        // Assert - Verifica colonne Owned Type per Recipient
        var columns = await _context.Database
            .SqlQuery<string>($"SELECT column_name FROM information_schema.columns WHERE table_name = 'notifications'")
            .ToListAsync();

        Assert.Contains("recipient_value", columns);
        Assert.Contains("recipient_channel", columns);
    }

    [Fact]
    public async Task Migrations_CreateIndexes()
    {
        // Act
        await _context.Database.MigrateAsync();

        // Assert - Verifica che gli indici esistano
        var indexes = await _context.Database
            .SqlQuery<string>($"SELECT indexname FROM pg_indexes WHERE tablename = 'notifications'")
            .ToListAsync();

        Assert.Contains("ix_notifications_status", indexes);
        Assert.Contains("ix_notifications_scheduled_at", indexes);
    }
}
