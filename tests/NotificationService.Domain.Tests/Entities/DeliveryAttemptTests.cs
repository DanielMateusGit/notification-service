using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;

namespace NotificationService.Domain.Tests.Entities;

public class DeliveryAttemptTests
{
    // ===== CONSTRUCTOR TESTS =====

    [Fact]
    public void Constructor_WithValidData_CreatesDeliveryAttempt()
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        var attemptNumber = 1;

        // Act
        var attempt = new DeliveryAttempt(notificationId, attemptNumber);

        // Assert
        Assert.NotEqual(Guid.Empty, attempt.Id);
        Assert.Equal(notificationId, attempt.NotificationId);
        Assert.Equal(attemptNumber, attempt.AttemptNumber);
        Assert.Equal(DeliveryStatus.InProgress, attempt.Status);
        Assert.Null(attempt.ErrorMessage);
        Assert.True(attempt.AttemptedAt <= DateTime.UtcNow);
        Assert.Null(attempt.CompletedAt);
    }

    [Fact]
    public void Constructor_WithEmptyNotificationId_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new DeliveryAttempt(Guid.Empty, 1));

        Assert.Equal("notificationId", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithZeroAttemptNumber_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new DeliveryAttempt(Guid.NewGuid(), 0));

        Assert.Equal("attemptNumber", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNegativeAttemptNumber_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new DeliveryAttempt(Guid.NewGuid(), -1));

        Assert.Equal("attemptNumber", exception.ParamName);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(5)]
    [InlineData(10)]
    public void Constructor_WithValidAttemptNumbers_CreatesAttempt(int attemptNumber)
    {
        // Arrange
        var notificationId = Guid.NewGuid();

        // Act
        var attempt = new DeliveryAttempt(notificationId, attemptNumber);

        // Assert
        Assert.Equal(attemptNumber, attempt.AttemptNumber);
    }

    // ===== MARK AS SUCCESS TESTS =====

    [Fact]
    public void MarkAsSuccess_WhenInProgress_ChangesStatusToSuccess()
    {
        // Arrange
        var attempt = new DeliveryAttempt(Guid.NewGuid(), 1);

        // Act
        attempt.MarkAsSuccess();

        // Assert
        Assert.Equal(DeliveryStatus.Success, attempt.Status);
        Assert.NotNull(attempt.CompletedAt);
        Assert.True(attempt.CompletedAt <= DateTime.UtcNow);
        Assert.Null(attempt.ErrorMessage);
    }

    [Fact]
    public void MarkAsSuccess_WhenAlreadySuccess_ThrowsInvalidOperationException()
    {
        // Arrange
        var attempt = new DeliveryAttempt(Guid.NewGuid(), 1);
        attempt.MarkAsSuccess();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            attempt.MarkAsSuccess());

        Assert.Contains("InProgress", exception.Message);
    }

    [Fact]
    public void MarkAsSuccess_WhenAlreadyFailed_ThrowsInvalidOperationException()
    {
        // Arrange
        var attempt = new DeliveryAttempt(Guid.NewGuid(), 1);
        attempt.MarkAsFailed("Some error");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => attempt.MarkAsSuccess());
    }

    // ===== MARK AS FAILED TESTS =====

    [Fact]
    public void MarkAsFailed_WhenInProgress_ChangesStatusToFailed()
    {
        // Arrange
        var attempt = new DeliveryAttempt(Guid.NewGuid(), 1);
        var errorMessage = "SMTP server timeout";

        // Act
        attempt.MarkAsFailed(errorMessage);

        // Assert
        Assert.Equal(DeliveryStatus.Failed, attempt.Status);
        Assert.Equal(errorMessage, attempt.ErrorMessage);
        Assert.NotNull(attempt.CompletedAt);
        Assert.True(attempt.CompletedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void MarkAsFailed_WithEmptyErrorMessage_ThrowsArgumentException()
    {
        // Arrange
        var attempt = new DeliveryAttempt(Guid.NewGuid(), 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            attempt.MarkAsFailed(""));

        Assert.Equal("errorMessage", exception.ParamName);
    }

    [Fact]
    public void MarkAsFailed_WithWhitespaceErrorMessage_ThrowsArgumentException()
    {
        // Arrange
        var attempt = new DeliveryAttempt(Guid.NewGuid(), 1);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => attempt.MarkAsFailed("   "));
    }

    [Fact]
    public void MarkAsFailed_WhenAlreadyFailed_ThrowsInvalidOperationException()
    {
        // Arrange
        var attempt = new DeliveryAttempt(Guid.NewGuid(), 1);
        attempt.MarkAsFailed("First error");

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            attempt.MarkAsFailed("Second error"));

        Assert.Contains("InProgress", exception.Message);
    }

    [Fact]
    public void MarkAsFailed_WhenAlreadySuccess_ThrowsInvalidOperationException()
    {
        // Arrange
        var attempt = new DeliveryAttempt(Guid.NewGuid(), 1);
        attempt.MarkAsSuccess();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            attempt.MarkAsFailed("Error after success"));
    }

    // ===== IS IN PROGRESS / IS COMPLETED TESTS =====

    [Fact]
    public void IsInProgress_WhenNewAttempt_ReturnsTrue()
    {
        // Arrange
        var attempt = new DeliveryAttempt(Guid.NewGuid(), 1);

        // Act & Assert
        Assert.True(attempt.IsInProgress);
        Assert.False(attempt.IsCompleted);
    }

    [Fact]
    public void IsInProgress_WhenSuccess_ReturnsFalse()
    {
        // Arrange
        var attempt = new DeliveryAttempt(Guid.NewGuid(), 1);
        attempt.MarkAsSuccess();

        // Act & Assert
        Assert.False(attempt.IsInProgress);
        Assert.True(attempt.IsCompleted);
    }

    [Fact]
    public void IsInProgress_WhenFailed_ReturnsFalse()
    {
        // Arrange
        var attempt = new DeliveryAttempt(Guid.NewGuid(), 1);
        attempt.MarkAsFailed("Error");

        // Act & Assert
        Assert.False(attempt.IsInProgress);
        Assert.True(attempt.IsCompleted);
    }

    // ===== GET DURATION TESTS =====

    [Fact]
    public void GetDuration_WhenInProgress_ReturnsNull()
    {
        // Arrange
        var attempt = new DeliveryAttempt(Guid.NewGuid(), 1);

        // Act
        var duration = attempt.GetDuration();

        // Assert
        Assert.Null(duration);
    }

    [Fact]
    public void GetDuration_WhenCompleted_ReturnsDuration()
    {
        // Arrange
        var attempt = new DeliveryAttempt(Guid.NewGuid(), 1);
        attempt.MarkAsSuccess();

        // Act
        var duration = attempt.GetDuration();

        // Assert
        Assert.NotNull(duration);
        Assert.True(duration.Value >= TimeSpan.Zero);
    }

    [Fact]
    public void GetDuration_WhenFailed_ReturnsDuration()
    {
        // Arrange
        var attempt = new DeliveryAttempt(Guid.NewGuid(), 1);
        attempt.MarkAsFailed("Error");

        // Act
        var duration = attempt.GetDuration();

        // Assert
        Assert.NotNull(duration);
        Assert.True(duration.Value >= TimeSpan.Zero);
    }

    // ===== MULTIPLE ATTEMPTS SCENARIO =====

    [Fact]
    public void MultipleAttempts_EachHasUniqueId()
    {
        // Arrange
        var notificationId = Guid.NewGuid();

        // Act
        var attempt1 = new DeliveryAttempt(notificationId, 1);
        var attempt2 = new DeliveryAttempt(notificationId, 2);
        var attempt3 = new DeliveryAttempt(notificationId, 3);

        // Assert
        Assert.NotEqual(attempt1.Id, attempt2.Id);
        Assert.NotEqual(attempt2.Id, attempt3.Id);
        Assert.NotEqual(attempt1.Id, attempt3.Id);

        // All share same NotificationId
        Assert.Equal(notificationId, attempt1.NotificationId);
        Assert.Equal(notificationId, attempt2.NotificationId);
        Assert.Equal(notificationId, attempt3.NotificationId);
    }

    [Fact]
    public void MultipleAttempts_SimulateRetryScenario()
    {
        // Arrange - Simula: 2 fallimenti, poi successo
        var notificationId = Guid.NewGuid();

        // Act
        var attempt1 = new DeliveryAttempt(notificationId, 1);
        attempt1.MarkAsFailed("Connection timeout");

        var attempt2 = new DeliveryAttempt(notificationId, 2);
        attempt2.MarkAsFailed("Server busy");

        var attempt3 = new DeliveryAttempt(notificationId, 3);
        attempt3.MarkAsSuccess();

        // Assert
        Assert.Equal(DeliveryStatus.Failed, attempt1.Status);
        Assert.Equal("Connection timeout", attempt1.ErrorMessage);

        Assert.Equal(DeliveryStatus.Failed, attempt2.Status);
        Assert.Equal("Server busy", attempt2.ErrorMessage);

        Assert.Equal(DeliveryStatus.Success, attempt3.Status);
        Assert.Null(attempt3.ErrorMessage);
    }
}
