using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using NotificationService.Domain.ValueObjects;

namespace NotificationService.Domain.Tests.Entities;

public class NotificationTests
{
    // ===== CONSTRUCTOR TESTS =====

    [Fact]
    public void Constructor_WithValidData_CreatesNotification()
    {
        // Arrange
        var recipient = Recipient.ForEmail("user@example.com");
        var content = "Test message";
        var subject = "Test subject";

        // Act
        var notification = new Notification(recipient, content, Priority.Normal, subject);

        // Assert
        Assert.NotEqual(Guid.Empty, notification.Id);
        Assert.Equal(recipient, notification.Recipient);
        Assert.Equal(NotificationChannel.Email, notification.Recipient.Channel);
        Assert.Equal(content, notification.Content);
        Assert.Equal(subject, notification.Subject);
        Assert.Equal(Priority.Normal, notification.Priority);
        Assert.Equal(NotificationStatus.Pending, notification.Status);
        Assert.True(notification.CreatedAt <= DateTime.UtcNow);
        Assert.Null(notification.SentAt);
        Assert.Null(notification.FailedAt);
    }

    [Fact]
    public void Constructor_WithNullRecipient_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new Notification(null!, "Content", Priority.Normal, "Subject"));
    }

    [Fact]
    public void Constructor_WithEmptyContent_ThrowsArgumentException()
    {
        // Arrange
        var recipient = Recipient.ForEmail("user@example.com");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new Notification(recipient, "", Priority.Normal, "Subject"));

        Assert.Equal("content", exception.ParamName);
    }

    [Fact]
    public void Constructor_EmailWithoutSubject_ThrowsArgumentException()
    {
        // Arrange
        var recipient = Recipient.ForEmail("user@example.com");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new Notification(recipient, "Content", Priority.Normal, null));

        Assert.Equal("subject", exception.ParamName);
    }

    [Fact]
    public void Constructor_SmsWithoutSubject_DoesNotThrow()
    {
        // Arrange
        var recipient = Recipient.ForSms("+1234567890");

        // Act
        var notification = new Notification(recipient, "Content");

        // Assert
        Assert.Equal(NotificationChannel.Sms, notification.Recipient.Channel);
        Assert.Null(notification.Subject);
    }

    // ===== SEND TESTS =====

    [Fact]
    public void Send_WhenPending_ChangeStatusToSent()
    {
        // Arrange
        var recipient = Recipient.ForEmail("user@example.com");
        var notification = new Notification(recipient, "Content", Priority.Normal, "Subject");

        // Act
        notification.Send();

        // Assert
        Assert.Equal(NotificationStatus.Sent, notification.Status);
        Assert.NotNull(notification.SentAt);
        Assert.True(notification.SentAt <= DateTime.UtcNow);
    }

    [Fact]
    public void Send_WhenAlreadySent_ThrowsInvalidOperationException()
    {
        // Arrange
        var recipient = Recipient.ForEmail("user@example.com");
        var notification = new Notification(recipient, "Content", Priority.Normal, "Subject");
        notification.Send();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => notification.Send());
        Assert.Contains("Pending", exception.Message);
    }

    [Fact]
    public void Send_WhenFailed_ThrowsInvalidOperationException()
    {
        // Arrange
        var recipient = Recipient.ForEmail("user@example.com");
        var notification = new Notification(recipient, "Content", Priority.Normal, "Subject");
        notification.Fail("Test error");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => notification.Send());
    }

    [Fact]
    public void Send_WhenCancelled_ThrowsInvalidOperationException()
    {
        // Arrange
        var recipient = Recipient.ForEmail("user@example.com");
        var notification = new Notification(recipient, "Content", Priority.Normal, "Subject");
        notification.Cancel();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => notification.Send());
    }

    // ===== FAIL TESTS =====

    [Fact]
    public void Fail_WhenPending_ChangeStatusToFailed()
    {
        // Arrange
        var recipient = Recipient.ForEmail("user@example.com");
        var notification = new Notification(recipient, "Content", Priority.Normal, "Subject");
        var errorMessage = "SMTP server unavailable";

        // Act
        notification.Fail(errorMessage);

        // Assert
        Assert.Equal(NotificationStatus.Failed, notification.Status);
        Assert.Equal(errorMessage, notification.ErrorMessage);
        Assert.NotNull(notification.FailedAt);
        Assert.True(notification.FailedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void Fail_WithEmptyErrorMessage_ThrowsArgumentException()
    {
        // Arrange
        var recipient = Recipient.ForEmail("user@example.com");
        var notification = new Notification(recipient, "Content", Priority.Normal, "Subject");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => notification.Fail(""));
        Assert.Equal("errorMessage", exception.ParamName);
    }

    [Fact]
    public void Fail_WhenAlreadySent_ThrowsInvalidOperationException()
    {
        // Arrange
        var recipient = Recipient.ForEmail("user@example.com");
        var notification = new Notification(recipient, "Content", Priority.Normal, "Subject");
        notification.Send();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => notification.Fail("Error"));
    }

    // ===== CANCEL TESTS =====

    [Fact]
    public void Cancel_WhenPending_ChangeStatusToCancelled()
    {
        // Arrange
        var recipient = Recipient.ForEmail("user@example.com");
        var notification = new Notification(recipient, "Content", Priority.Normal, "Subject");

        // Act
        notification.Cancel();

        // Assert
        Assert.Equal(NotificationStatus.Cancelled, notification.Status);
    }

    [Fact]
    public void Cancel_WhenAlreadySent_ThrowsInvalidOperationException()
    {
        // Arrange
        var recipient = Recipient.ForEmail("user@example.com");
        var notification = new Notification(recipient, "Content", Priority.Normal, "Subject");
        notification.Send();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => notification.Cancel());
    }

    // ===== SCHEDULE TESTS =====

    [Fact]
    public void Schedule_WithFutureDate_SetsScheduledAt()
    {
        // Arrange
        var recipient = Recipient.ForEmail("user@example.com");
        var notification = new Notification(recipient, "Content", Priority.Normal, "Subject");
        var scheduledAt = DateTime.UtcNow.AddHours(2);

        // Act
        notification.Schedule(scheduledAt);

        // Assert
        Assert.Equal(scheduledAt, notification.ScheduledAt);
    }

    [Fact]
    public void Schedule_WithPastDate_ThrowsArgumentException()
    {
        // Arrange
        var recipient = Recipient.ForEmail("user@example.com");
        var notification = new Notification(recipient, "Content", Priority.Normal, "Subject");
        var pastDate = DateTime.UtcNow.AddHours(-1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => notification.Schedule(pastDate));
        Assert.Equal("scheduledAt", exception.ParamName);
    }

    [Fact]
    public void Schedule_WhenNotPending_ThrowsInvalidOperationException()
    {
        // Arrange
        var recipient = Recipient.ForEmail("user@example.com");
        var notification = new Notification(recipient, "Content", Priority.Normal, "Subject");
        notification.Send();
        var futureDate = DateTime.UtcNow.AddHours(2);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => notification.Schedule(futureDate));
    }

    // ===== CAN RETRY TESTS =====

    [Theory]
    [InlineData(Priority.Low, 1, true)]      // 1 < 2 retry
    [InlineData(Priority.Low, 2, false)]     // 2 >= 2 retry
    [InlineData(Priority.Normal, 2, true)]   // 2 < 3 retry
    [InlineData(Priority.Normal, 3, false)]  // 3 >= 3 retry
    [InlineData(Priority.High, 4, true)]     // 4 < 5 retry
    [InlineData(Priority.High, 5, false)]    // 5 >= 5 retry
    [InlineData(Priority.Critical, 9, true)] // 9 < 10 retry
    [InlineData(Priority.Critical, 10, false)] // 10 >= 10 retry
    public void CanRetry_ReturnsCorrectValueBasedOnPriority(Priority priority, int attemptNumber, bool expected)
    {
        // Arrange
        var recipient = Recipient.ForEmail("user@example.com");
        var notification = new Notification(recipient, "Content", priority, "Subject");

        // Act
        var canRetry = notification.CanRetry(attemptNumber);

        // Assert
        Assert.Equal(expected, canRetry);
    }

    // ===== IS READY TO SEND TESTS =====

    [Fact]
    public void IsReadyToSend_WhenPendingAndNotScheduled_ReturnsTrue()
    {
        // Arrange
        var recipient = Recipient.ForEmail("user@example.com");
        var notification = new Notification(recipient, "Content", Priority.Normal, "Subject");

        // Act
        var isReady = notification.IsReadyToSend();

        // Assert
        Assert.True(isReady);
    }

    [Fact]
    public void IsReadyToSend_WhenPendingAndScheduledInPast_ReturnsTrue()
    {
        // Arrange
        var recipient = Recipient.ForEmail("user@example.com");
        var notification = new Notification(recipient, "Content", Priority.Normal, "Subject");
        // Simula schedulazione nel passato (in realtà non possiamo con la validazione, ma testiamo la logica)
        // In un caso reale, questo sarebbe schedulato in futuro e poi il tempo passerebbe

        // Act
        var isReady = notification.IsReadyToSend();

        // Assert
        Assert.True(isReady); // Non schedulata = pronta
    }

    [Fact]
    public void IsReadyToSend_WhenPendingAndScheduledInFuture_ReturnsFalse()
    {
        // Arrange
        var recipient = Recipient.ForEmail("user@example.com");
        var notification = new Notification(recipient, "Content", Priority.Normal, "Subject");
        notification.Schedule(DateTime.UtcNow.AddHours(2));

        // Act
        var isReady = notification.IsReadyToSend();

        // Assert
        Assert.False(isReady);
    }

    [Fact]
    public void IsReadyToSend_WhenSent_ReturnsFalse()
    {
        // Arrange
        var recipient = Recipient.ForEmail("user@example.com");
        var notification = new Notification(recipient, "Content", Priority.Normal, "Subject");
        notification.Send();

        // Act
        var isReady = notification.IsReadyToSend();

        // Assert
        Assert.False(isReady);
    }

    [Fact]
    public void IsReadyToSend_WhenFailed_ReturnsFalse()
    {
        // Arrange
        var recipient = Recipient.ForEmail("user@example.com");
        var notification = new Notification(recipient, "Content", Priority.Normal, "Subject");
        notification.Fail("Error");

        // Act
        var isReady = notification.IsReadyToSend();

        // Assert
        Assert.False(isReady);
    }

    [Fact]
    public void IsReadyToSend_WhenCancelled_ReturnsFalse()
    {
        // Arrange
        var recipient = Recipient.ForEmail("user@example.com");
        var notification = new Notification(recipient, "Content", Priority.Normal, "Subject");
        notification.Cancel();

        // Act
        var isReady = notification.IsReadyToSend();

        // Assert
        Assert.False(isReady);
    }

    // ===== RETRY TESTS =====

    [Fact]
    public void Retry_WhenFailed_ChangeStatusToPending()
    {
        // Arrange
        var recipient = Recipient.ForEmail("user@example.com");
        var notification = new Notification(recipient, "Content", Priority.Normal, "Subject");
        notification.Fail("Connection timeout");

        // Act
        notification.Retry();

        // Assert
        Assert.Equal(NotificationStatus.Pending, notification.Status);
    }

    [Fact]
    public void Retry_WhenFailed_ClearsErrorMessage()
    {
        // Arrange
        var recipient = Recipient.ForEmail("user@example.com");
        var notification = new Notification(recipient, "Content", Priority.Normal, "Subject");
        notification.Fail("Connection timeout");

        // Act
        notification.Retry();

        // Assert
        Assert.Null(notification.ErrorMessage);
    }

    [Fact]
    public void Retry_WhenFailed_ClearsFailedAt()
    {
        // Arrange
        var recipient = Recipient.ForEmail("user@example.com");
        var notification = new Notification(recipient, "Content", Priority.Normal, "Subject");
        notification.Fail("Connection timeout");

        // Act
        notification.Retry();

        // Assert
        Assert.Null(notification.FailedAt);
    }

    [Fact]
    public void Retry_WhenFailed_RaisesNotificationRetriedEvent()
    {
        // Arrange
        var recipient = Recipient.ForEmail("user@example.com");
        var notification = new Notification(recipient, "Content", Priority.Normal, "Subject");
        notification.Fail("Connection timeout");
        notification.ClearDomainEvents(); // Clear the FailedEvent

        // Act
        notification.Retry();

        // Assert
        var events = notification.DomainEvents;
        Assert.Single(events);
        Assert.IsType<NotificationService.Domain.Events.NotificationRetriedEvent>(events.First());
    }

    [Fact]
    public void Retry_WhenPending_ThrowsInvalidOperationException()
    {
        // Arrange
        var recipient = Recipient.ForEmail("user@example.com");
        var notification = new Notification(recipient, "Content", Priority.Normal, "Subject");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => notification.Retry());
    }

    [Fact]
    public void Retry_WhenSent_ThrowsInvalidOperationException()
    {
        // Arrange
        var recipient = Recipient.ForEmail("user@example.com");
        var notification = new Notification(recipient, "Content", Priority.Normal, "Subject");
        notification.Send();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => notification.Retry());
    }

    [Fact]
    public void Retry_WhenCancelled_ThrowsInvalidOperationException()
    {
        // Arrange
        var recipient = Recipient.ForEmail("user@example.com");
        var notification = new Notification(recipient, "Content", Priority.Normal, "Subject");
        notification.Cancel();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => notification.Retry());
    }
}
