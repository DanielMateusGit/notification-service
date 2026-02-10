using NotificationService.Domain.ValueObjects;

namespace NotificationService.Domain.Tests.ValueObjects;

public class EmailAddressTests
{
    // ===== CONSTRUCTOR / VALIDATION TESTS =====

    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@domain.org")]
    [InlineData("user+tag@example.co.uk")]
    [InlineData("firstname.lastname@company.com")]
    public void Constructor_WithValidEmail_CreatesEmailAddress(string email)
    {
        // Act
        var emailAddress = new EmailAddress(email);

        // Assert
        Assert.Equal(email.ToLowerInvariant(), emailAddress.Value);
    }

    [Fact]
    public void Constructor_NormalizesToLowercase()
    {
        // Arrange
        var email = "Test.User@EXAMPLE.COM";

        // Act
        var emailAddress = new EmailAddress(email);

        // Assert
        Assert.Equal("test.user@example.com", emailAddress.Value);
    }

    [Fact]
    public void Constructor_TrimsWhitespace()
    {
        // Arrange
        var email = "  test@example.com  ";

        // Act
        var emailAddress = new EmailAddress(email);

        // Assert
        Assert.Equal("test@example.com", emailAddress.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithEmptyEmail_ThrowsArgumentException(string? email)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new EmailAddress(email!));
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("missing@domain")]
    [InlineData("@nodomain.com")]
    [InlineData("spaces in@email.com")]
    [InlineData("double@@at.com")]
    public void Constructor_WithInvalidEmail_ThrowsArgumentException(string email)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new EmailAddress(email));
        Assert.Contains("Invalid email format", exception.Message);
    }

    // ===== DOMAIN / LOCAL PART TESTS =====

    [Fact]
    public void Domain_ReturnsCorrectDomain()
    {
        // Arrange
        var emailAddress = new EmailAddress("user@example.com");

        // Act & Assert
        Assert.Equal("example.com", emailAddress.Domain);
    }

    [Fact]
    public void LocalPart_ReturnsCorrectLocalPart()
    {
        // Arrange
        var emailAddress = new EmailAddress("user.name@example.com");

        // Act & Assert
        Assert.Equal("user.name", emailAddress.LocalPart);
    }

    // ===== EQUALITY TESTS =====

    [Fact]
    public void Equals_WithSameEmail_ReturnsTrue()
    {
        // Arrange
        var email1 = new EmailAddress("test@example.com");
        var email2 = new EmailAddress("test@example.com");

        // Act & Assert
        Assert.True(email1.Equals(email2));
        Assert.True(email1 == email2);
        Assert.Equal(email1.GetHashCode(), email2.GetHashCode());
    }

    [Fact]
    public void Equals_WithDifferentCase_ReturnsTrue()
    {
        // Arrange (normalized to lowercase)
        var email1 = new EmailAddress("Test@Example.com");
        var email2 = new EmailAddress("test@example.com");

        // Act & Assert
        Assert.True(email1 == email2);
    }

    [Fact]
    public void Equals_WithDifferentEmail_ReturnsFalse()
    {
        // Arrange
        var email1 = new EmailAddress("test1@example.com");
        var email2 = new EmailAddress("test2@example.com");

        // Act & Assert
        Assert.False(email1.Equals(email2));
        Assert.True(email1 != email2);
    }

    [Fact]
    public void Equals_WithNull_ReturnsFalse()
    {
        // Arrange
        var email = new EmailAddress("test@example.com");

        // Act & Assert
        Assert.False(email.Equals(null));
    }

    // ===== CONVERSION TESTS =====

    [Fact]
    public void ToString_ReturnsValue()
    {
        // Arrange
        var email = new EmailAddress("test@example.com");

        // Act & Assert
        Assert.Equal("test@example.com", email.ToString());
    }

    [Fact]
    public void ImplicitConversion_ToString_Works()
    {
        // Arrange
        var email = new EmailAddress("test@example.com");

        // Act
        string value = email;

        // Assert
        Assert.Equal("test@example.com", value);
    }
}
