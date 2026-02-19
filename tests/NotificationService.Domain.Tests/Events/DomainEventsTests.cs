using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using NotificationService.Domain.Events;
using NotificationService.Domain.ValueObjects;

namespace NotificationService.Domain.Tests.Events;

public class DomainEventsTests
{
    // ===== ENTITY BASE CLASS TESTS =====

    [Fact]
    public void Entity_InitiallyHasNoEvents()
    {
        // Arrange & Act
        var notification = CreateValidNotification();

        // Assert
        Assert.Empty(notification.DomainEvents);
    }

    [Fact]
    public void ClearDomainEvents_RemovesAllEvents()
    {
        // Arrange
        var notification = CreateValidNotification();
        notification.Send();
        Assert.NotEmpty(notification.DomainEvents);

        // Act
        notification.ClearDomainEvents();

        // Assert
        Assert.Empty(notification.DomainEvents);
    }

    // ===== NOTIFICATION SCHEDULED EVENT =====

    [Fact]
    public void Schedule_RaisesNotificationScheduledEvent()
    {
        // Arrange
        var notification = CreateValidNotification();
        var scheduledAt = DateTime.UtcNow.AddHours(1);

        // Act
        notification.Schedule(scheduledAt);

        // Assert
        Assert.Single(notification.DomainEvents);
        var @event = Assert.IsType<NotificationScheduledEvent>(notification.DomainEvents[0]);
        Assert.Equal(notification.Id, @event.NotificationId);
        Assert.Equal(notification.Recipient, @event.Recipient);
        Assert.Equal(scheduledAt, @event.ScheduledAt);
    }

    [Fact]
    public void Schedule_EventHasOccurredOnTimestamp()
    {
        // Arrange
        var notification = CreateValidNotification();
        var before = DateTime.UtcNow;

        // Act
        notification.Schedule(DateTime.UtcNow.AddHours(1));
        var after = DateTime.UtcNow;

        // Assert
        var @event = (NotificationScheduledEvent)notification.DomainEvents[0];
        Assert.InRange(@event.OccurredOn, before, after);
    }

    // ===== NOTIFICATION SENT EVENT =====

    [Fact]
    public void Send_RaisesNotificationSentEvent()
    {
        // Arrange
        var notification = CreateValidNotification();

        // Act
        notification.Send();

        // Assert
        Assert.Single(notification.DomainEvents);
        var @event = Assert.IsType<NotificationSentEvent>(notification.DomainEvents[0]);
        Assert.Equal(notification.Id, @event.NotificationId);
        Assert.Equal(notification.Recipient, @event.Recipient);
        Assert.Equal(notification.SentAt, @event.SentAt);
    }

    [Fact]
    public void Send_EventHasOccurredOnTimestamp()
    {
        // Arrange
        var notification = CreateValidNotification();
        var before = DateTime.UtcNow;

        // Act
        notification.Send();
        var after = DateTime.UtcNow;

        // Assert
        var @event = (NotificationSentEvent)notification.DomainEvents[0];
        Assert.InRange(@event.OccurredOn, before, after);
    }

    [Fact]
    public void Send_EventSentAtMatchesNotificationSentAt()
    {
        // Arrange
        var notification = CreateValidNotification();

        // Act
        notification.Send();

        // Assert
        var @event = (NotificationSentEvent)notification.DomainEvents[0];
        Assert.Equal(notification.SentAt, @event.SentAt);
    }

    // ===== NOTIFICATION FAILED EVENT =====

    [Fact]
    public void Fail_RaisesNotificationFailedEvent()
    {
        // Arrange
        var notification = CreateValidNotification();
        var errorMessage = "SMTP timeout";

        // Act
        notification.Fail(errorMessage);

        // Assert
        Assert.Single(notification.DomainEvents);
        var @event = Assert.IsType<NotificationFailedEvent>(notification.DomainEvents[0]);
        Assert.Equal(notification.Id, @event.NotificationId);
        Assert.Equal(notification.Recipient, @event.Recipient);
        Assert.Equal(errorMessage, @event.ErrorMessage);
        Assert.Equal(notification.FailedAt, @event.FailedAt);
    }

    [Fact]
    public void Fail_EventHasOccurredOnTimestamp()
    {
        // Arrange
        var notification = CreateValidNotification();
        var before = DateTime.UtcNow;

        // Act
        notification.Fail("Error");
        var after = DateTime.UtcNow;

        // Assert
        var @event = (NotificationFailedEvent)notification.DomainEvents[0];
        Assert.InRange(@event.OccurredOn, before, after);
    }

    [Fact]
    public void Fail_EventFailedAtMatchesNotificationFailedAt()
    {
        // Arrange
        var notification = CreateValidNotification();

        // Act
        notification.Fail("Connection refused");

        // Assert
        var @event = (NotificationFailedEvent)notification.DomainEvents[0];
        Assert.Equal(notification.FailedAt, @event.FailedAt);
    }

    // ===== MULTIPLE EVENTS =====

    [Fact]
    public void MultipleActions_AccumulateEvents()
    {
        // Arrange
        var notification1 = CreateValidNotification();
        var notification2 = CreateValidNotification();

        // Act
        notification1.Schedule(DateTime.UtcNow.AddHours(1));
        notification2.Send();

        // Assert
        Assert.Single(notification1.DomainEvents);
        Assert.IsType<NotificationScheduledEvent>(notification1.DomainEvents[0]);

        Assert.Single(notification2.DomainEvents);
        Assert.IsType<NotificationSentEvent>(notification2.DomainEvents[0]);
    }

    // ===== CANCEL DOES NOT RAISE EVENT =====

    [Fact]
    public void Cancel_DoesNotRaiseEvent()
    {
        // Arrange
        var notification = CreateValidNotification();

        // Act
        notification.Cancel();

        // Assert
        Assert.Empty(notification.DomainEvents);
    }

    // ===== EVENT IMMUTABILITY =====

    [Fact]
    public void NotificationSentEvent_IsImmutable()
    {
        // Arrange
        var notification = CreateValidNotification();
        notification.Send();
        var @event = (NotificationSentEvent)notification.DomainEvents[0];

        // Assert - record properties are init-only, this is a compile-time guarantee
        // We just verify the record was created correctly
        Assert.NotEqual(Guid.Empty, @event.NotificationId);
        Assert.NotNull(@event.Recipient);
        Assert.True(@event.SentAt > DateTime.MinValue);
    }

    // ===== HELPERS =====

    private static Notification CreateValidNotification(
        NotificationChannel channel = NotificationChannel.Email,
        Priority priority = Priority.Normal)
    {
        var recipient = channel switch
        {
            NotificationChannel.Email => Recipient.ForEmail("test@example.com"),
            NotificationChannel.Sms => Recipient.ForSms("+1234567890"),
            NotificationChannel.Push => Recipient.ForPush("user123"),
            NotificationChannel.Webhook => Recipient.ForWebhook("https://webhook.example.com/notify"),
            _ => Recipient.ForEmail("test@example.com")
        };

        return new Notification(
            recipient: recipient,
            content: "Test content",
            priority: priority,
            subject: channel == NotificationChannel.Email ? "Test Subject" : null
        );
    }
}
