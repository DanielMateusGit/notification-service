using NSubstitute;
using NotificationService.Application.Commands.Notifications;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;

namespace NotificationService.Application.Tests.Handlers;

public class RetryNotificationHandlerTests
{
    private readonly INotificationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly RetryNotificationHandler _handler;

    public RetryNotificationHandlerTests()
    {
        _repository = Substitute.For<INotificationRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new RetryNotificationHandler(_repository, _unitOfWork);
    }

    private static Notification CreateFailedNotification()
    {
        var notification = new Notification(
            recipient: "user@example.com",
            channel: NotificationChannel.Email,
            content: "Test",
            subject: "Subject"
        );
        // Fail it so we can retry
        notification.Fail("Test error");
        return notification;
    }

    [Fact]
    public async Task Handle_ExistingFailedNotification_ReturnsTrue()
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        var notification = CreateFailedNotification();

        _repository.GetByIdAsync(notificationId, Arg.Any<CancellationToken>())
            .Returns(notification);

        var command = new RetryNotificationCommand(notificationId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task Handle_NonExistingNotification_ReturnsFalse()
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        _repository.GetByIdAsync(notificationId, Arg.Any<CancellationToken>())
            .Returns((Notification?)null);

        var command = new RetryNotificationCommand(notificationId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task Handle_NonExistingNotification_DoesNotCallUpdate()
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        _repository.GetByIdAsync(notificationId, Arg.Any<CancellationToken>())
            .Returns((Notification?)null);

        var command = new RetryNotificationCommand(notificationId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _repository.DidNotReceive().UpdateAsync(
            Arg.Any<Notification>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NonExistingNotification_DoesNotCallSaveChanges()
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        _repository.GetByIdAsync(notificationId, Arg.Any<CancellationToken>())
            .Returns((Notification?)null);

        var command = new RetryNotificationCommand(notificationId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ExistingFailedNotification_CallsRepositoryUpdate()
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        var notification = CreateFailedNotification();

        _repository.GetByIdAsync(notificationId, Arg.Any<CancellationToken>())
            .Returns(notification);

        var command = new RetryNotificationCommand(notificationId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _repository.Received(1).UpdateAsync(notification, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ExistingFailedNotification_CallsUnitOfWorkSaveChanges()
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        var notification = CreateFailedNotification();

        _repository.GetByIdAsync(notificationId, Arg.Any<CancellationToken>())
            .Returns(notification);

        var command = new RetryNotificationCommand(notificationId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ExistingFailedNotification_NotificationStatusIsPending()
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        var notification = CreateFailedNotification();
        Assert.Equal(NotificationStatus.Failed, notification.Status); // Precondition

        _repository.GetByIdAsync(notificationId, Arg.Any<CancellationToken>())
            .Returns(notification);

        var command = new RetryNotificationCommand(notificationId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(NotificationStatus.Pending, notification.Status);
    }

    [Fact]
    public async Task Handle_ExistingFailedNotification_ErrorMessageIsCleared()
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        var notification = CreateFailedNotification();
        Assert.NotNull(notification.ErrorMessage); // Precondition

        _repository.GetByIdAsync(notificationId, Arg.Any<CancellationToken>())
            .Returns(notification);

        var command = new RetryNotificationCommand(notificationId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Null(notification.ErrorMessage);
    }
}
