using NSubstitute;
using NotificationService.Application.Interfaces;
using NotificationService.Application.Queries.Notifications;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;

namespace NotificationService.Application.Tests.Handlers;

public class GetNotificationsByStatusHandlerTests
{
    private readonly INotificationRepository _repository;
    private readonly GetNotificationsByStatusHandler _handler;

    public GetNotificationsByStatusHandlerTests()
    {
        _repository = Substitute.For<INotificationRepository>();
        _handler = new GetNotificationsByStatusHandler(_repository);
    }

    [Fact]
    public async Task Handle_WithMatchingNotifications_ReturnsDtoList()
    {
        // Arrange
        var notifications = new List<Notification>
        {
            new("user1@example.com", NotificationChannel.Email, "Content 1", subject: "Subject 1"),
            new("user2@example.com", NotificationChannel.Email, "Content 2", subject: "Subject 2")
        };

        _repository.GetByStatusAsync(NotificationStatus.Pending, Arg.Any<CancellationToken>())
            .Returns(notifications);

        var query = new GetNotificationsByStatusQuery(NotificationStatus.Pending);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task Handle_WithNoMatchingNotifications_ReturnsEmptyList()
    {
        // Arrange
        _repository.GetByStatusAsync(NotificationStatus.Sent, Arg.Any<CancellationToken>())
            .Returns(new List<Notification>());

        var query = new GetNotificationsByStatusQuery(NotificationStatus.Sent);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_MapsAllFieldsCorrectly()
    {
        // Arrange
        var notification = new Notification(
            recipient: "user@example.com",
            channel: NotificationChannel.Sms,
            content: "SMS content",
            priority: Priority.Critical
        );

        _repository.GetByStatusAsync(NotificationStatus.Pending, Arg.Any<CancellationToken>())
            .Returns(new List<Notification> { notification });

        var query = new GetNotificationsByStatusQuery(NotificationStatus.Pending);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var dto = result.Single();
        Assert.Equal(notification.Id, dto.Id);
        Assert.Equal(notification.Recipient, dto.Recipient);
        Assert.Equal("Sms", dto.Channel);
        Assert.Equal(notification.Content, dto.Content);
        Assert.Equal("Pending", dto.Status);
        Assert.Equal("Critical", dto.Priority);
    }

    [Fact]
    public async Task Handle_CallsRepositoryWithCorrectStatus()
    {
        // Arrange
        _repository.GetByStatusAsync(Arg.Any<NotificationStatus>(), Arg.Any<CancellationToken>())
            .Returns(new List<Notification>());

        var query = new GetNotificationsByStatusQuery(NotificationStatus.Failed);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        await _repository.Received(1).GetByStatusAsync(NotificationStatus.Failed, Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(NotificationStatus.Pending)]
    [InlineData(NotificationStatus.Sent)]
    [InlineData(NotificationStatus.Failed)]
    [InlineData(NotificationStatus.Cancelled)]
    public async Task Handle_DifferentStatuses_CallsRepositoryCorrectly(NotificationStatus status)
    {
        // Arrange
        _repository.GetByStatusAsync(status, Arg.Any<CancellationToken>())
            .Returns(new List<Notification>());

        var query = new GetNotificationsByStatusQuery(status);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        await _repository.Received(1).GetByStatusAsync(status, Arg.Any<CancellationToken>());
    }
}
