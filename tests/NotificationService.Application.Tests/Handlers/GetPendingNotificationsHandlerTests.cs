using NSubstitute;
using NotificationService.Application.Interfaces;
using NotificationService.Application.Queries.Notifications;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using NotificationService.Domain.ValueObjects;

namespace NotificationService.Application.Tests.Handlers;

public class GetPendingNotificationsHandlerTests
{
    private readonly INotificationRepository _repository;
    private readonly GetPendingNotificationsHandler _handler;

    public GetPendingNotificationsHandlerTests()
    {
        _repository = Substitute.For<INotificationRepository>();
        _handler = new GetPendingNotificationsHandler(_repository);
    }

    [Fact]
    public async Task Handle_WithReadyNotifications_ReturnsDtoList()
    {
        // Arrange - notifiche non schedulate sono sempre "ready"
        var notifications = new List<Notification>
        {
            new(Recipient.ForEmail("user1@example.com"), "Content 1", subject: "Subject 1"),
            new(Recipient.ForEmail("user2@example.com"), "Content 2", subject: "Subject 2")
        };

        _repository.GetPendingAsync(Arg.Any<CancellationToken>())
            .Returns(notifications);

        var query = new GetPendingNotificationsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task Handle_WithNoNotifications_ReturnsEmptyList()
    {
        // Arrange
        _repository.GetPendingAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Notification>());

        var query = new GetPendingNotificationsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_FiltersNotificationsNotReadyToSend()
    {
        // Arrange
        var readyNotification = new Notification(
            Recipient.ForEmail("ready@example.com"),
            "Ready",
            subject: "Subject"
        );
        // Non schedulata = ready to send

        var futureNotification = new Notification(
            Recipient.ForEmail("future@example.com"),
            "Future",
            subject: "Subject"
        );
        futureNotification.Schedule(DateTime.UtcNow.AddHours(5)); // Schedulata nel futuro = NOT ready

        var notifications = new List<Notification> { readyNotification, futureNotification };

        _repository.GetPendingAsync(Arg.Any<CancellationToken>())
            .Returns(notifications);

        var query = new GetPendingNotificationsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Single(result);
        Assert.Equal("ready@example.com", result[0].Recipient);
    }

    [Fact]
    public async Task Handle_MapsAllFieldsCorrectly()
    {
        // Arrange
        var recipient = Recipient.ForSms("+391234567890");
        var notification = new Notification(
            recipient: recipient,
            content: "SMS content",
            priority: Priority.High
        );

        _repository.GetPendingAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Notification> { notification });

        var query = new GetPendingNotificationsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var dto = result.Single();
        Assert.Equal(notification.Id, dto.Id);
        Assert.Equal(notification.Recipient.Value, dto.Recipient);
        Assert.Equal("Sms", dto.Channel);
        Assert.Equal(notification.Content, dto.Content);
        Assert.Equal("Pending", dto.Status);
        Assert.Equal("High", dto.Priority);
    }

    [Fact]
    public async Task Handle_CallsRepositoryGetPending()
    {
        // Arrange
        _repository.GetPendingAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Notification>());

        var query = new GetPendingNotificationsQuery();

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        await _repository.Received(1).GetPendingAsync(Arg.Any<CancellationToken>());
    }
}
