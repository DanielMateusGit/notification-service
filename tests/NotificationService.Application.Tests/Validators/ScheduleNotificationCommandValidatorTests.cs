using FluentValidation.TestHelper;
using NotificationService.Application.Commands.Notifications;
using NotificationService.Application.Validators;
using NotificationService.Domain.Enums;

namespace NotificationService.Application.Tests.Validators;

public class ScheduleNotificationCommandValidatorTests
{
    private readonly ScheduleNotificationCommandValidator _validator = new();

    // ===== VALID COMMANDS =====

    [Fact]
    public void Validate_ValidEmailCommand_ShouldPass()
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
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ValidSmsCommand_ShouldPass()
    {
        // Arrange
        var command = new ScheduleNotificationCommand(
            Recipient: "+391234567890",
            Channel: NotificationChannel.Sms,
            Content: "Test SMS",
            ScheduledAt: DateTime.UtcNow.AddHours(1)
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ValidWebhookCommand_ShouldPass()
    {
        // Arrange
        var command = new ScheduleNotificationCommand(
            Recipient: "https://api.example.com/webhook",
            Channel: NotificationChannel.Webhook,
            Content: "{\"event\": \"test\"}",
            ScheduledAt: DateTime.UtcNow.AddHours(1)
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ValidPushCommand_ShouldPass()
    {
        // Arrange
        var command = new ScheduleNotificationCommand(
            Recipient: "device-token-123",
            Channel: NotificationChannel.Push,
            Content: "Push notification",
            ScheduledAt: DateTime.UtcNow.AddHours(1)
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    // ===== RECIPIENT VALIDATION =====

    [Fact]
    public void Validate_EmptyRecipient_ShouldFail()
    {
        // Arrange
        var command = new ScheduleNotificationCommand(
            Recipient: "",
            Channel: NotificationChannel.Email,
            Content: "Content",
            ScheduledAt: DateTime.UtcNow.AddHours(1),
            Subject: "Subject"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Recipient)
            .WithErrorMessage("Recipient is required");
    }

    [Fact]
    public void Validate_RecipientTooLong_ShouldFail()
    {
        // Arrange
        var command = new ScheduleNotificationCommand(
            Recipient: new string('a', 256) + "@example.com",
            Channel: NotificationChannel.Email,
            Content: "Content",
            ScheduledAt: DateTime.UtcNow.AddHours(1),
            Subject: "Subject"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Recipient)
            .WithErrorMessage("Recipient must not exceed 255 characters");
    }

    // ===== EMAIL CHANNEL SPECIFIC =====

    [Fact]
    public void Validate_EmailChannel_InvalidEmail_ShouldFail()
    {
        // Arrange
        var command = new ScheduleNotificationCommand(
            Recipient: "not-an-email",
            Channel: NotificationChannel.Email,
            Content: "Content",
            ScheduledAt: DateTime.UtcNow.AddHours(1),
            Subject: "Subject"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Recipient)
            .WithErrorMessage("Invalid email format");
    }

    [Fact]
    public void Validate_EmailChannel_MissingSubject_ShouldFail()
    {
        // Arrange
        var command = new ScheduleNotificationCommand(
            Recipient: "user@example.com",
            Channel: NotificationChannel.Email,
            Content: "Content",
            ScheduledAt: DateTime.UtcNow.AddHours(1),
            Subject: null
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Subject)
            .WithErrorMessage("Subject is required for email notifications");
    }

    [Fact]
    public void Validate_EmailChannel_EmptySubject_ShouldFail()
    {
        // Arrange
        var command = new ScheduleNotificationCommand(
            Recipient: "user@example.com",
            Channel: NotificationChannel.Email,
            Content: "Content",
            ScheduledAt: DateTime.UtcNow.AddHours(1),
            Subject: ""
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Subject)
            .WithErrorMessage("Subject is required for email notifications");
    }

    // ===== SMS CHANNEL SPECIFIC =====

    [Fact]
    public void Validate_SmsChannel_InvalidPhoneNumber_ShouldFail()
    {
        // Arrange
        var command = new ScheduleNotificationCommand(
            Recipient: "not-a-phone",
            Channel: NotificationChannel.Sms,
            Content: "SMS content",
            ScheduledAt: DateTime.UtcNow.AddHours(1)
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Recipient)
            .WithErrorMessage("Invalid phone number format");
    }

    [Theory]
    [InlineData("+391234567890")]   // Italy
    [InlineData("+14155551234")]    // USA
    [InlineData("447911123456")]    // UK without +
    public void Validate_SmsChannel_ValidPhoneNumbers_ShouldPass(string phoneNumber)
    {
        // Arrange
        var command = new ScheduleNotificationCommand(
            Recipient: phoneNumber,
            Channel: NotificationChannel.Sms,
            Content: "SMS content",
            ScheduledAt: DateTime.UtcNow.AddHours(1)
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Recipient);
    }

    // ===== WEBHOOK CHANNEL SPECIFIC =====

    [Fact]
    public void Validate_WebhookChannel_InvalidUrl_ShouldFail()
    {
        // Arrange
        var command = new ScheduleNotificationCommand(
            Recipient: "not-a-url",
            Channel: NotificationChannel.Webhook,
            Content: "{}",
            ScheduledAt: DateTime.UtcNow.AddHours(1)
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Recipient)
            .WithErrorMessage("Invalid webhook URL");
    }

    [Fact]
    public void Validate_WebhookChannel_FtpUrl_ShouldFail()
    {
        // Arrange - FTP non è accettato, solo HTTP/HTTPS
        var command = new ScheduleNotificationCommand(
            Recipient: "ftp://server.com/file",
            Channel: NotificationChannel.Webhook,
            Content: "{}",
            ScheduledAt: DateTime.UtcNow.AddHours(1)
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Recipient)
            .WithErrorMessage("Invalid webhook URL");
    }

    [Theory]
    [InlineData("https://api.example.com/webhook")]
    [InlineData("http://localhost:5000/callback")]
    public void Validate_WebhookChannel_ValidUrls_ShouldPass(string url)
    {
        // Arrange
        var command = new ScheduleNotificationCommand(
            Recipient: url,
            Channel: NotificationChannel.Webhook,
            Content: "{}",
            ScheduledAt: DateTime.UtcNow.AddHours(1)
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Recipient);
    }

    // ===== CONTENT VALIDATION =====

    [Fact]
    public void Validate_EmptyContent_ShouldFail()
    {
        // Arrange
        var command = new ScheduleNotificationCommand(
            Recipient: "user@example.com",
            Channel: NotificationChannel.Email,
            Content: "",
            ScheduledAt: DateTime.UtcNow.AddHours(1),
            Subject: "Subject"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Content)
            .WithErrorMessage("Content is required");
    }

    [Fact]
    public void Validate_ContentTooLong_ShouldFail()
    {
        // Arrange
        var command = new ScheduleNotificationCommand(
            Recipient: "user@example.com",
            Channel: NotificationChannel.Email,
            Content: new string('x', 4001),
            ScheduledAt: DateTime.UtcNow.AddHours(1),
            Subject: "Subject"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Content)
            .WithErrorMessage("Content must not exceed 4000 characters");
    }

    // ===== SCHEDULED AT VALIDATION =====

    [Fact]
    public void Validate_ScheduledAtInPast_ShouldFail()
    {
        // Arrange
        var command = new ScheduleNotificationCommand(
            Recipient: "user@example.com",
            Channel: NotificationChannel.Email,
            Content: "Content",
            ScheduledAt: DateTime.UtcNow.AddHours(-1),
            Subject: "Subject"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ScheduledAt)
            .WithErrorMessage("ScheduledAt must be in the future");
    }

    // ===== SUBJECT VALIDATION =====

    [Fact]
    public void Validate_SubjectTooLong_ShouldFail()
    {
        // Arrange
        var command = new ScheduleNotificationCommand(
            Recipient: "user@example.com",
            Channel: NotificationChannel.Email,
            Content: "Content",
            ScheduledAt: DateTime.UtcNow.AddHours(1),
            Subject: new string('x', 256)
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Subject)
            .WithErrorMessage("Subject must not exceed 255 characters");
    }

    // ===== NON-EMAIL CHANNELS DON'T REQUIRE SUBJECT =====

    [Theory]
    [InlineData(NotificationChannel.Sms)]
    [InlineData(NotificationChannel.Push)]
    [InlineData(NotificationChannel.Webhook)]
    public void Validate_NonEmailChannel_SubjectNotRequired(NotificationChannel channel)
    {
        // Arrange
        var recipient = channel switch
        {
            NotificationChannel.Sms => "+391234567890",
            NotificationChannel.Webhook => "https://example.com/webhook",
            _ => "device-token"
        };

        var command = new ScheduleNotificationCommand(
            Recipient: recipient,
            Channel: channel,
            Content: "Content",
            ScheduledAt: DateTime.UtcNow.AddHours(1),
            Subject: null
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Subject);
    }
}
