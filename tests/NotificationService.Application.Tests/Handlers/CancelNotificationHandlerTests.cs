using NSubstitute;
using NotificationService.Application.Commands.Notifications;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using NotificationService.Domain.ValueObjects;

namespace NotificationService.Application.Tests.Handlers;

public class CancelNotificationHandlerTests
{
    private readonly INotificationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CancelNotificationHandler _handler;

    public CancelNotificationHandlerTests()
    {
        _repository = Substitute.For<INotificationRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new CancelNotificationHandler(_repository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_ExistingPendingNotification_ReturnsTrue()
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        var recipient = Recipient.ForEmail("user@example.com");
        var notification = new Notification(
            recipient: recipient,
            content: "Test",
            subject: "Subject"
        );

        _repository.GetByIdAsync(notificationId, Arg.Any<CancellationToken>())
            .Returns(notification);

        var command = new CancelNotificationCommand(notificationId);

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

        var command = new CancelNotificationCommand(notificationId);

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

        var command = new CancelNotificationCommand(notificationId);

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

        var command = new CancelNotificationCommand(notificationId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ExistingNotification_CallsRepositoryUpdate()
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        var recipient = Recipient.ForEmail("user@example.com");
        var notification = new Notification(
            recipient: recipient,
            content: "Test",
            subject: "Subject"
        );

        _repository.GetByIdAsync(notificationId, Arg.Any<CancellationToken>())
            .Returns(notification);

        var command = new CancelNotificationCommand(notificationId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _repository.Received(1).UpdateAsync(notification, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ExistingNotification_CallsUnitOfWorkSaveChanges()
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        var recipient = Recipient.ForEmail("user@example.com");
        var notification = new Notification(
            recipient: recipient,
            content: "Test",
            subject: "Subject"
        );

        _repository.GetByIdAsync(notificationId, Arg.Any<CancellationToken>())
            .Returns(notification);

        var command = new CancelNotificationCommand(notificationId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ExistingNotification_NotificationStatusIsCancelled()
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        var recipient = Recipient.ForEmail("user@example.com");
        var notification = new Notification(
            recipient: recipient,
            content: "Test",
            subject: "Subject"
        );

        _repository.GetByIdAsync(notificationId, Arg.Any<CancellationToken>())
            .Returns(notification);

        var command = new CancelNotificationCommand(notificationId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(NotificationStatus.Cancelled, notification.Status);
    }
}
