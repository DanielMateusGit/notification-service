using NSubstitute;
using NotificationService.Application.Commands.Notifications;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;

namespace NotificationService.Application.Tests.Handlers;

public class ScheduleNotificationHandlerTests
{
    private readonly INotificationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ScheduleNotificationHandler _handler;

    public ScheduleNotificationHandlerTests()
    {
        _repository = Substitute.For<INotificationRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new ScheduleNotificationHandler(_repository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsNewGuid()
    {
        // Arrange
        var command = new ScheduleNotificationCommand(
            Recipient: "user@example.com",
            Channel: NotificationChannel.Email,
            Content: "Test content",
            ScheduledAt: DateTime.UtcNow.AddHours(1),
            Priority: Priority.Normal,
            Subject: "Test Subject"
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result);
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsRepositoryAddAsync()
    {
        // Arrange
        var command = new ScheduleNotificationCommand(
            Recipient: "user@example.com",
            Channel: NotificationChannel.Email,
            Content: "Test content",
            ScheduledAt: DateTime.UtcNow.AddHours(1),
            Subject: "Test Subject"
        );

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _repository.Received(1).AddAsync(
            Arg.Is<Notification>(n =>
                n.Recipient == command.Recipient &&
                n.Channel == command.Channel &&
                n.Content == command.Content &&
                n.Subject == command.Subject),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsUnitOfWorkSaveChanges()
    {
        // Arrange
        var command = new ScheduleNotificationCommand(
            Recipient: "user@example.com",
            Channel: NotificationChannel.Email,
            Content: "Test content",
            ScheduledAt: DateTime.UtcNow.AddHours(1),
            Subject: "Test Subject"
        );

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ValidCommand_NotificationHasScheduledAt()
    {
        // Arrange
        var scheduledAt = DateTime.UtcNow.AddHours(2);
        var command = new ScheduleNotificationCommand(
            Recipient: "user@example.com",
            Channel: NotificationChannel.Email,
            Content: "Test content",
            ScheduledAt: scheduledAt,
            Subject: "Test Subject"
        );

        Notification? capturedNotification = null;
        await _repository.AddAsync(
            Arg.Do<Notification>(n => capturedNotification = n),
            Arg.Any<CancellationToken>());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedNotification);
        Assert.Equal(scheduledAt, capturedNotification!.ScheduledAt);
    }

    [Theory]
    [InlineData(Priority.Low)]
    [InlineData(Priority.Normal)]
    [InlineData(Priority.High)]
    [InlineData(Priority.Critical)]
    public async Task Handle_DifferentPriorities_SetsCorrectPriority(Priority priority)
    {
        // Arrange
        var command = new ScheduleNotificationCommand(
            Recipient: "user@example.com",
            Channel: NotificationChannel.Email,
            Content: "Test content",
            ScheduledAt: DateTime.UtcNow.AddHours(1),
            Priority: priority,
            Subject: "Test Subject"
        );

        Notification? capturedNotification = null;
        await _repository.AddAsync(
            Arg.Do<Notification>(n => capturedNotification = n),
            Arg.Any<CancellationToken>());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedNotification);
        Assert.Equal(priority, capturedNotification!.Priority);
    }

    [Theory]
    [InlineData(NotificationChannel.Email)]
    [InlineData(NotificationChannel.Sms)]
    [InlineData(NotificationChannel.Push)]
    [InlineData(NotificationChannel.Webhook)]
    public async Task Handle_DifferentChannels_SetsCorrectChannel(NotificationChannel channel)
    {
        // Arrange
        var recipient = channel switch
        {
            NotificationChannel.Email => "user@example.com",
            NotificationChannel.Sms => "+391234567890",
            NotificationChannel.Webhook => "https://example.com/webhook",
            _ => "device-token"
        };

        var command = new ScheduleNotificationCommand(
            Recipient: recipient,
            Channel: channel,
            Content: "Test content",
            ScheduledAt: DateTime.UtcNow.AddHours(1),
            Subject: channel == NotificationChannel.Email ? "Subject" : null
        );

        Notification? capturedNotification = null;
        await _repository.AddAsync(
            Arg.Do<Notification>(n => capturedNotification = n),
            Arg.Any<CancellationToken>());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedNotification);
        Assert.Equal(channel, capturedNotification!.Channel);
    }
}
