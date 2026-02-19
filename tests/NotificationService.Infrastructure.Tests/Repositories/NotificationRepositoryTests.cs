using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using NotificationService.Domain.ValueObjects;
using NotificationService.Infrastructure.Persistence.Repositories;
using NotificationService.Infrastructure.Tests.Fixtures;

namespace NotificationService.Infrastructure.Tests.Repositories;

/// <summary>
/// Integration tests per PostgresNotificationRepository.
/// Usa un database PostgreSQL reale tramite Testcontainers.
/// </summary>
public class NotificationRepositoryTests : IClassFixture<DatabaseFixture>, IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;
    private readonly PostgresNotificationRepository _repository;

    public NotificationRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _repository = new PostgresNotificationRepository(_fixture.Context);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        // Pulisce il database dopo ogni test
        await _fixture.CleanupAsync();
    }

    // ===== ADD TESTS =====

    [Fact]
    public async Task AddAsync_SavesNotificationToDatabase()
    {
        // Arrange
        var recipient = Recipient.ForEmail("test@example.com");
        var notification = new Notification(recipient, "Test content", subject: "Test subject");

        // Act
        await _repository.AddAsync(notification);
        await _fixture.Context.SaveChangesAsync();

        // Assert - Verifica che sia nel database
        var saved = await _fixture.Context.Notifications.FindAsync(notification.Id);
        Assert.NotNull(saved);
        Assert.Equal(notification.Id, saved.Id);
    }

    [Fact]
    public async Task AddAsync_PersistsRecipientValueObject()
    {
        // Arrange
        var recipient = Recipient.ForEmail("recipient@example.com");
        var notification = new Notification(recipient, "Content", subject: "Subject");

        // Act
        await _repository.AddAsync(notification);
        await _fixture.Context.SaveChangesAsync();

        // Assert - Verifica che Recipient VO sia persistito correttamente
        var saved = await _fixture.Context.Notifications.FindAsync(notification.Id);
        Assert.NotNull(saved);
        Assert.Equal("recipient@example.com", saved.Recipient.Value);
        Assert.Equal(NotificationChannel.Email, saved.Recipient.Channel);
    }

    [Fact]
    public async Task AddAsync_PersistsSmsRecipient()
    {
        // Arrange
        var recipient = Recipient.ForSms("+391234567890");
        var notification = new Notification(recipient, "SMS content");

        // Act
        await _repository.AddAsync(notification);
        await _fixture.Context.SaveChangesAsync();

        // Assert
        var saved = await _fixture.Context.Notifications.FindAsync(notification.Id);
        Assert.NotNull(saved);
        Assert.Equal("+391234567890", saved.Recipient.Value);
        Assert.Equal(NotificationChannel.Sms, saved.Recipient.Channel);
    }

    // ===== GET BY ID TESTS =====

    [Fact]
    public async Task GetByIdAsync_ExistingNotification_ReturnsNotification()
    {
        // Arrange
        var recipient = Recipient.ForEmail("find@example.com");
        var notification = new Notification(recipient, "Find me", subject: "Subject");
        await _repository.AddAsync(notification);
        await _fixture.Context.SaveChangesAsync();

        // Detach per simulare una nuova query
        _fixture.Context.ChangeTracker.Clear();

        // Act
        var found = await _repository.GetByIdAsync(notification.Id);

        // Assert
        Assert.NotNull(found);
        Assert.Equal(notification.Id, found.Id);
        Assert.Equal("find@example.com", found.Recipient.Value);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingNotification_ReturnsNull()
    {
        // Act
        var found = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(found);
    }

    // ===== GET BY STATUS TESTS =====

    [Fact]
    public async Task GetByStatusAsync_ReturnsOnlyMatchingStatus()
    {
        // Arrange
        var pending = new Notification(Recipient.ForEmail("pending@example.com"), "Pending", subject: "S");
        var sent = new Notification(Recipient.ForEmail("sent@example.com"), "Sent", subject: "S");
        sent.Send();

        await _repository.AddAsync(pending);
        await _repository.AddAsync(sent);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var pendingNotifications = await _repository.GetByStatusAsync(NotificationStatus.Pending);
        var sentNotifications = await _repository.GetByStatusAsync(NotificationStatus.Sent);

        // Assert
        Assert.Single(pendingNotifications);
        Assert.Equal("pending@example.com", pendingNotifications.First().Recipient.Value);

        Assert.Single(sentNotifications);
        Assert.Equal("sent@example.com", sentNotifications.First().Recipient.Value);
    }

    // ===== GET PENDING TESTS =====

    [Fact]
    public async Task GetPendingAsync_ReturnsOnlyPendingNotifications()
    {
        // Arrange
        var pending1 = new Notification(Recipient.ForEmail("p1@example.com"), "P1", subject: "S");
        var pending2 = new Notification(Recipient.ForEmail("p2@example.com"), "P2", subject: "S");
        var sent = new Notification(Recipient.ForEmail("sent@example.com"), "Sent", subject: "S");
        sent.Send();

        await _repository.AddAsync(pending1);
        await _repository.AddAsync(pending2);
        await _repository.AddAsync(sent);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var pending = await _repository.GetPendingAsync();

        // Assert
        Assert.Equal(2, pending.Count);
        Assert.All(pending, n => Assert.Equal(NotificationStatus.Pending, n.Status));
    }

    // ===== UPDATE TESTS =====

    [Fact]
    public async Task UpdateAsync_PersistsChanges()
    {
        // Arrange
        var notification = new Notification(
            Recipient.ForEmail("update@example.com"),
            "Original",
            subject: "Subject"
        );
        await _repository.AddAsync(notification);
        await _fixture.Context.SaveChangesAsync();

        // Act - Modifica lo stato
        notification.Send();
        await _repository.UpdateAsync(notification);
        await _fixture.Context.SaveChangesAsync();

        // Clear tracker e ricarica
        _fixture.Context.ChangeTracker.Clear();
        var updated = await _repository.GetByIdAsync(notification.Id);

        // Assert
        Assert.NotNull(updated);
        Assert.Equal(NotificationStatus.Sent, updated.Status);
        Assert.NotNull(updated.SentAt);
    }

}
