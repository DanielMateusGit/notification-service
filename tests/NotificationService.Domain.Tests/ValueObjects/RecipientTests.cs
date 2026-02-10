using NotificationService.Domain.Enums;
using NotificationService.Domain.ValueObjects;

namespace NotificationService.Domain.Tests.ValueObjects;

public class RecipientTests
{
    // ===== FOR EMAIL TESTS =====

    [Fact]
    public void ForEmail_WithValidEmail_CreatesRecipient()
    {
        // Arrange
        var email = "test@example.com";

        // Act
        var recipient = Recipient.ForEmail(email);

        // Assert
        Assert.Equal(email, recipient.Value);
        Assert.Equal(NotificationChannel.Email, recipient.Channel);
        Assert.NotNull(recipient.Email);
        Assert.Equal(email, recipient.Email.Value);
        Assert.Null(recipient.Phone);
    }

    [Fact]
    public void ForEmail_WithInvalidEmail_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Recipient.ForEmail("invalid"));
    }

    // ===== FOR SMS TESTS =====

    [Fact]
    public void ForSms_WithValidPhone_CreatesRecipient()
    {
        // Arrange
        var phone = "+391234567890";

        // Act
        var recipient = Recipient.ForSms(phone);

        // Assert
        Assert.Equal(phone, recipient.Value);
        Assert.Equal(NotificationChannel.Sms, recipient.Channel);
        Assert.NotNull(recipient.Phone);
        Assert.Equal(phone, recipient.Phone.Value);
        Assert.Null(recipient.Email);
    }

    [Fact]
    public void ForSms_WithInvalidPhone_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Recipient.ForSms("invalid"));
    }

    // ===== FOR PUSH TESTS =====

    [Fact]
    public void ForPush_WithValidUserId_CreatesRecipient()
    {
        // Arrange
        var userId = "user-123-abc";

        // Act
        var recipient = Recipient.ForPush(userId);

        // Assert
        Assert.Equal(userId, recipient.Value);
        Assert.Equal(NotificationChannel.Push, recipient.Channel);
        Assert.Null(recipient.Email);
        Assert.Null(recipient.Phone);
    }

    [Fact]
    public void ForPush_TrimsUserId()
    {
        // Arrange
        var userId = "  user-123  ";

        // Act
        var recipient = Recipient.ForPush(userId);

        // Assert
        Assert.Equal("user-123", recipient.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void ForPush_WithEmptyUserId_ThrowsArgumentException(string? userId)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Recipient.ForPush(userId!));
    }

    // ===== FOR WEBHOOK TESTS =====

    [Fact]
    public void ForWebhook_WithValidHttpsUrl_CreatesRecipient()
    {
        // Arrange
        var url = "https://api.example.com/webhooks/notify";

        // Act
        var recipient = Recipient.ForWebhook(url);

        // Assert
        Assert.Equal(url, recipient.Value);
        Assert.Equal(NotificationChannel.Webhook, recipient.Channel);
    }

    [Fact]
    public void ForWebhook_WithValidHttpUrl_CreatesRecipient()
    {
        // Arrange
        var url = "http://localhost:3000/webhook";

        // Act
        var recipient = Recipient.ForWebhook(url);

        // Assert
        Assert.Contains("http://localhost", recipient.Value);
        Assert.Equal(NotificationChannel.Webhook, recipient.Channel);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void ForWebhook_WithEmptyUrl_ThrowsArgumentException(string? url)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Recipient.ForWebhook(url!));
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("ftp://invalid.com/path")]
    [InlineData("file:///local/path")]
    public void ForWebhook_WithInvalidUrl_ThrowsArgumentException(string url)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Recipient.ForWebhook(url));
    }

    // ===== CREATE FACTORY TESTS =====

    [Fact]
    public void Create_WithEmailChannel_CreatesEmailRecipient()
    {
        // Act
        var recipient = Recipient.Create("test@example.com", NotificationChannel.Email);

        // Assert
        Assert.Equal(NotificationChannel.Email, recipient.Channel);
        Assert.NotNull(recipient.Email);
    }

    [Fact]
    public void Create_WithSmsChannel_CreatesSmsRecipient()
    {
        // Act
        var recipient = Recipient.Create("+391234567890", NotificationChannel.Sms);

        // Assert
        Assert.Equal(NotificationChannel.Sms, recipient.Channel);
        Assert.NotNull(recipient.Phone);
    }

    [Fact]
    public void Create_WithPushChannel_CreatesPushRecipient()
    {
        // Act
        var recipient = Recipient.Create("user-123", NotificationChannel.Push);

        // Assert
        Assert.Equal(NotificationChannel.Push, recipient.Channel);
    }

    [Fact]
    public void Create_WithWebhookChannel_CreatesWebhookRecipient()
    {
        // Act
        var recipient = Recipient.Create("https://example.com/hook", NotificationChannel.Webhook);

        // Assert
        Assert.Equal(NotificationChannel.Webhook, recipient.Channel);
    }

    // ===== EQUALITY TESTS =====

    [Fact]
    public void Equals_WithSameValueAndChannel_ReturnsTrue()
    {
        // Arrange
        var recipient1 = Recipient.ForEmail("test@example.com");
        var recipient2 = Recipient.ForEmail("test@example.com");

        // Act & Assert
        Assert.True(recipient1.Equals(recipient2));
        Assert.True(recipient1 == recipient2);
        Assert.Equal(recipient1.GetHashCode(), recipient2.GetHashCode());
    }

    [Fact]
    public void Equals_WithDifferentValue_ReturnsFalse()
    {
        // Arrange
        var recipient1 = Recipient.ForEmail("test1@example.com");
        var recipient2 = Recipient.ForEmail("test2@example.com");

        // Act & Assert
        Assert.False(recipient1.Equals(recipient2));
        Assert.True(recipient1 != recipient2);
    }

    [Fact]
    public void Equals_WithDifferentChannel_ReturnsFalse()
    {
        // Arrange
        var recipient1 = Recipient.ForPush("user-123");
        var recipient2 = Recipient.ForWebhook("https://user-123.com/hook");

        // Act & Assert
        Assert.False(recipient1.Equals(recipient2));
    }

    [Fact]
    public void Equals_WithNull_ReturnsFalse()
    {
        // Arrange
        var recipient = Recipient.ForEmail("test@example.com");

        // Act & Assert
        Assert.False(recipient.Equals(null));
    }

    // ===== CONVERSION TESTS =====

    [Fact]
    public void ToString_ReturnsChannelAndValue()
    {
        // Arrange
        var recipient = Recipient.ForEmail("test@example.com");

        // Act
        var result = recipient.ToString();

        // Assert
        Assert.Equal("Email: test@example.com", result);
    }

    [Fact]
    public void ImplicitConversion_ToString_ReturnsValue()
    {
        // Arrange
        var recipient = Recipient.ForEmail("test@example.com");

        // Act
        string value = recipient;

        // Assert
        Assert.Equal("test@example.com", value);
    }
}
