using NSubstitute;
using NotificationService.Application.Interfaces;
using NotificationService.Application.Queries.Notifications;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;

namespace NotificationService.Application.Tests.Handlers;

public class GetNotificationByIdHandlerTests
{
    private readonly INotificationRepository _repository;
    private readonly GetNotificationByIdHandler _handler;

    public GetNotificationByIdHandlerTests()
    {
        _repository = Substitute.For<INotificationRepository>();
        _handler = new GetNotificationByIdHandler(_repository);
    }

    [Fact]
    public async Task Handle_ExistingNotification_ReturnsDto()
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        var notification = new Notification(
            recipient: "user@example.com",
            channel: NotificationChannel.Email,
            content: "Test content",
            subject: "Subject"
        );

        _repository.GetByIdAsync(notificationId, Arg.Any<CancellationToken>())
            .Returns(notification);

        var query = new GetNotificationByIdQuery(notificationId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(notification.Recipient, result!.Recipient);
        Assert.Equal(notification.Content, result.Content);
        Assert.Equal(notification.Subject, result.Subject);
    }

    [Fact]
    public async Task Handle_NonExistingNotification_ReturnsNull()
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        _repository.GetByIdAsync(notificationId, Arg.Any<CancellationToken>())
            .Returns((Notification?)null);

        var query = new GetNotificationByIdQuery(notificationId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_ExistingNotification_MapsAllFieldsCorrectly()
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        var notification = new Notification(
            recipient: "user@example.com",
            channel: NotificationChannel.Email,
            content: "Test content",
            priority: Priority.High,
            subject: "Test Subject"
        );
        notification.Schedule(DateTime.UtcNow.AddHours(1));

        _repository.GetByIdAsync(notificationId, Arg.Any<CancellationToken>())
            .Returns(notification);

        var query = new GetNotificationByIdQuery(notificationId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(notification.Id, result!.Id);
        Assert.Equal(notification.Recipient, result.Recipient);
        Assert.Equal("Email", result.Channel);
        Assert.Equal(notification.Content, result.Content);
        Assert.Equal(notification.Subject, result.Subject);
        Assert.Equal("Pending", result.Status);
        Assert.Equal("High", result.Priority);
        Assert.Equal(notification.CreatedAt, result.CreatedAt);
        Assert.Equal(notification.ScheduledAt, result.ScheduledAt);
    }

    [Fact]
    public async Task Handle_CallsRepositoryWithCorrectId()
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        _repository.GetByIdAsync(notificationId, Arg.Any<CancellationToken>())
            .Returns((Notification?)null);

        var query = new GetNotificationByIdQuery(notificationId);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        await _repository.Received(1).GetByIdAsync(notificationId, Arg.Any<CancellationToken>());
    }
}
