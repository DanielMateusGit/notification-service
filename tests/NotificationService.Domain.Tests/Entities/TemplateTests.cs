using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using NotificationService.Domain.ValueObjects;

namespace NotificationService.Domain.Tests.Entities;

public class TemplateTests
{
    // ===== CONSTRUCTOR TESTS =====

    [Fact]
    public void Constructor_WithValidData_CreatesTemplate()
    {
        // Arrange
        var name = "order-shipped";
        var channel = NotificationChannel.Email;
        var body = "Your order {{orderId}} has been shipped!";
        var subject = "Order {{orderId}} Shipped";

        // Act
        var template = new Template(name, channel, body, subject);

        // Assert
        Assert.NotEqual(Guid.Empty, template.Id);
        Assert.Equal("order-shipped", template.Name); // normalizzato
        Assert.Equal(channel, template.Channel);
        Assert.Equal(body, template.Body);
        Assert.Equal(subject, template.Subject);
        Assert.True(template.IsActive);
        Assert.True(template.CreatedAt <= DateTime.UtcNow);
        Assert.Null(template.UpdatedAt);
    }

    [Fact]
    public void Constructor_NormalizesName_ToLowerCase()
    {
        // Arrange & Act
        var template = new Template("ORDER-SHIPPED", NotificationChannel.Sms, "Body");

        // Assert
        Assert.Equal("order-shipped", template.Name);
    }

    [Fact]
    public void Constructor_TrimsName()
    {
        // Arrange & Act
        var template = new Template("  order-shipped  ", NotificationChannel.Sms, "Body");

        // Assert
        Assert.Equal("order-shipped", template.Name);
    }

    [Fact]
    public void Constructor_WithEmptyName_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new Template("", NotificationChannel.Sms, "Body"));

        Assert.Equal("name", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithEmptyBody_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new Template("test-template", NotificationChannel.Sms, ""));

        Assert.Equal("body", exception.ParamName);
    }

    [Fact]
    public void Constructor_EmailWithoutSubject_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new Template("test-template", NotificationChannel.Email, "Body", null));

        Assert.Equal("subject", exception.ParamName);
    }

    [Fact]
    public void Constructor_SmsWithoutSubject_DoesNotThrow()
    {
        // Act
        var template = new Template("test-template", NotificationChannel.Sms, "Body");

        // Assert
        Assert.Equal(NotificationChannel.Sms, template.Channel);
        Assert.Null(template.Subject);
    }

    // ===== RENDER TESTS =====

    [Fact]
    public void Render_WithValidData_ReplacesPlaceholders()
    {
        // Arrange
        var template = new Template(
            "order-shipped",
            NotificationChannel.Email,
            "Hello {{userName}}, your order {{orderId}} is on the way!",
            "Order {{orderId}} Shipped");

        var data = new TemplateData(new Dictionary<string, string>
        {
            ["userName"] = "Mario",
            ["orderId"] = "12345"
        });

        // Act
        var (subject, body) = template.Render(data);

        // Assert
        Assert.Equal("Order 12345 Shipped", subject);
        Assert.Equal("Hello Mario, your order 12345 is on the way!", body);
    }

    [Fact]
    public void Render_WithNoPlaceholders_ReturnsOriginalText()
    {
        // Arrange
        var template = new Template(
            "simple",
            NotificationChannel.Email,
            "This is a simple message",
            "Simple Subject");

        var data = TemplateData.Empty;

        // Act
        var (subject, body) = template.Render(data);

        // Assert
        Assert.Equal("Simple Subject", subject);
        Assert.Equal("This is a simple message", body);
    }

    [Fact]
    public void Render_WithMissingPlaceholder_ThrowsArgumentException()
    {
        // Arrange
        var template = new Template(
            "test",
            NotificationChannel.Email,
            "Hello {{userName}}!",
            "Subject");

        var data = TemplateData.Empty; // manca userName

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => template.Render(data));
        Assert.Contains("userName", exception.Message);
    }

    [Fact]
    public void Render_WhenInactive_ThrowsInvalidOperationException()
    {
        // Arrange
        var template = new Template(
            "test",
            NotificationChannel.Email,
            "Hello {{userName}}!",
            "Subject");
        template.Deactivate();

        var data = new TemplateData(new Dictionary<string, string> { ["userName"] = "Test" });

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => template.Render(data));
        Assert.Contains("inactive", exception.Message.ToLower());
    }

    [Fact]
    public void Render_SmsWithoutSubject_ReturnsNullSubject()
    {
        // Arrange
        var template = new Template("sms-template", NotificationChannel.Sms, "Your code: {{code}}");
        var data = new TemplateData(new Dictionary<string, string> { ["code"] = "1234" });

        // Act
        var (subject, body) = template.Render(data);

        // Assert
        Assert.Null(subject);
        Assert.Equal("Your code: 1234", body);
    }

    [Fact]
    public void Render_WithSamePlaceholderMultipleTimes_ReplacesAll()
    {
        // Arrange
        var template = new Template(
            "test",
            NotificationChannel.Email,
            "{{name}} is great. I love {{name}}!",
            "Hi {{name}}");

        var data = new TemplateData(new Dictionary<string, string> { ["name"] = "Mario" });

        // Act
        var (subject, body) = template.Render(data);

        // Assert
        Assert.Equal("Hi Mario", subject);
        Assert.Equal("Mario is great. I love Mario!", body);
    }

    // ===== ACTIVATE / DEACTIVATE TESTS =====

    [Fact]
    public void Deactivate_WhenActive_SetsIsActiveToFalse()
    {
        // Arrange
        var template = new Template("test", NotificationChannel.Sms, "Body");
        Assert.True(template.IsActive);

        // Act
        template.Deactivate();

        // Assert
        Assert.False(template.IsActive);
        Assert.NotNull(template.UpdatedAt);
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_IsIdempotent()
    {
        // Arrange
        var template = new Template("test", NotificationChannel.Sms, "Body");
        template.Deactivate();
        var firstUpdate = template.UpdatedAt;

        // Act
        template.Deactivate(); // chiamato di nuovo

        // Assert
        Assert.False(template.IsActive);
        Assert.Equal(firstUpdate, template.UpdatedAt); // non cambia
    }

    [Fact]
    public void Activate_WhenInactive_SetsIsActiveToTrue()
    {
        // Arrange
        var template = new Template("test", NotificationChannel.Sms, "Body");
        template.Deactivate();

        // Act
        template.Activate();

        // Assert
        Assert.True(template.IsActive);
        Assert.NotNull(template.UpdatedAt);
    }

    [Fact]
    public void Activate_WhenAlreadyActive_IsIdempotent()
    {
        // Arrange
        var template = new Template("test", NotificationChannel.Sms, "Body");
        Assert.True(template.IsActive);
        var originalUpdatedAt = template.UpdatedAt; // null

        // Act
        template.Activate();

        // Assert
        Assert.True(template.IsActive);
        Assert.Null(template.UpdatedAt); // non cambia
    }

    // ===== UPDATE CONTENT TESTS =====

    [Fact]
    public void UpdateContent_WithValidData_UpdatesBodyAndSubject()
    {
        // Arrange
        var template = new Template(
            "test",
            NotificationChannel.Email,
            "Old body",
            "Old subject");

        // Act
        template.UpdateContent("New body", "New subject");

        // Assert
        Assert.Equal("New body", template.Body);
        Assert.Equal("New subject", template.Subject);
        Assert.NotNull(template.UpdatedAt);
    }

    [Fact]
    public void UpdateContent_WithEmptyBody_ThrowsArgumentException()
    {
        // Arrange
        var template = new Template("test", NotificationChannel.Sms, "Body");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            template.UpdateContent(""));

        Assert.Equal("body", exception.ParamName);
    }

    [Fact]
    public void UpdateContent_EmailWithoutSubject_ThrowsArgumentException()
    {
        // Arrange
        var template = new Template("test", NotificationChannel.Email, "Body", "Subject");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            template.UpdateContent("New body", null));

        Assert.Equal("subject", exception.ParamName);
    }

    [Fact]
    public void UpdateContent_SmsWithoutSubject_DoesNotThrow()
    {
        // Arrange
        var template = new Template("test", NotificationChannel.Sms, "Old body");

        // Act
        template.UpdateContent("New body");

        // Assert
        Assert.Equal("New body", template.Body);
        Assert.Null(template.Subject);
    }

    // ===== GET PLACEHOLDERS TESTS =====

    [Fact]
    public void GetPlaceholders_ReturnsAllUniquePlaceholders()
    {
        // Arrange
        var template = new Template(
            "test",
            NotificationChannel.Email,
            "{{userName}} ordered {{orderId}}. Thanks {{userName}}!",
            "Order {{orderId}}");

        // Act
        var placeholders = template.GetPlaceholders();

        // Assert
        Assert.Equal(2, placeholders.Count);
        Assert.Contains("userName", placeholders);
        Assert.Contains("orderId", placeholders);
    }

    [Fact]
    public void GetPlaceholders_WithNoPlaceholders_ReturnsEmptyList()
    {
        // Arrange
        var template = new Template(
            "simple",
            NotificationChannel.Sms,
            "This is a simple message");

        // Act
        var placeholders = template.GetPlaceholders();

        // Assert
        Assert.Empty(placeholders);
    }

    [Fact]
    public void GetPlaceholders_IncludesSubjectPlaceholders()
    {
        // Arrange
        var template = new Template(
            "test",
            NotificationChannel.Email,
            "Body with {{bodyVar}}",
            "Subject with {{subjectVar}}");

        // Act
        var placeholders = template.GetPlaceholders();

        // Assert
        Assert.Equal(2, placeholders.Count);
        Assert.Contains("bodyVar", placeholders);
        Assert.Contains("subjectVar", placeholders);
    }
}
