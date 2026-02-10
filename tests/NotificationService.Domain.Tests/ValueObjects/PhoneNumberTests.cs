using NotificationService.Domain.ValueObjects;

namespace NotificationService.Domain.Tests.ValueObjects;

public class PhoneNumberTests
{
    // ===== CONSTRUCTOR / VALIDATION TESTS =====

    [Theory]
    [InlineData("+391234567890")]
    [InlineData("+14155551234")]
    [InlineData("+442071234567")]
    [InlineData("+8613812345678")]
    public void Constructor_WithValidE164_CreatesPhoneNumber(string phone)
    {
        // Act
        var phoneNumber = new PhoneNumber(phone);

        // Assert
        Assert.Equal(phone, phoneNumber.Value);
    }

    [Fact]
    public void Constructor_NormalizesSpacesAndDashes()
    {
        // Arrange
        var phone = "+39 123 456-7890";

        // Act
        var phoneNumber = new PhoneNumber(phone);

        // Assert
        Assert.Equal("+391234567890", phoneNumber.Value);
    }

    [Fact]
    public void Constructor_NormalizesParentheses()
    {
        // Arrange
        var phone = "+39 (123) 456-7890";

        // Act
        var phoneNumber = new PhoneNumber(phone);

        // Assert
        Assert.Equal("+391234567890", phoneNumber.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithEmptyPhone_ThrowsArgumentException(string? phone)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new PhoneNumber(phone!));
    }

    [Theory]
    [InlineData("1234567890")]           // Missing +
    [InlineData("+0123456789")]          // Starts with 0 after +
    [InlineData("+39123")]               // Too short
    [InlineData("+391234567890123456")]  // Too long (>15 digits)
    [InlineData("not-a-number")]
    public void Constructor_WithInvalidPhone_ThrowsArgumentException(string phone)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new PhoneNumber(phone));
        Assert.Contains("Invalid phone number format", exception.Message);
    }

    // ===== COUNTRY CODE / NATIONAL NUMBER TESTS =====

    [Fact]
    public void CountryCode_ReturnsFirstTwoDigits()
    {
        // Arrange
        var phoneNumber = new PhoneNumber("+391234567890");

        // Act & Assert
        Assert.Equal("39", phoneNumber.CountryCode);
    }

    [Fact]
    public void NationalNumber_ReturnsNumberWithoutCountryCode()
    {
        // Arrange
        var phoneNumber = new PhoneNumber("+391234567890");

        // Act & Assert
        Assert.Equal("1234567890", phoneNumber.NationalNumber);
    }

    // ===== EQUALITY TESTS =====

    [Fact]
    public void Equals_WithSameNumber_ReturnsTrue()
    {
        // Arrange
        var phone1 = new PhoneNumber("+391234567890");
        var phone2 = new PhoneNumber("+391234567890");

        // Act & Assert
        Assert.True(phone1.Equals(phone2));
        Assert.True(phone1 == phone2);
        Assert.Equal(phone1.GetHashCode(), phone2.GetHashCode());
    }

    [Fact]
    public void Equals_WithSameNumberDifferentFormat_ReturnsTrue()
    {
        // Arrange (normalized)
        var phone1 = new PhoneNumber("+39 123 456 7890");
        var phone2 = new PhoneNumber("+391234567890");

        // Act & Assert
        Assert.True(phone1 == phone2);
    }

    [Fact]
    public void Equals_WithDifferentNumber_ReturnsFalse()
    {
        // Arrange
        var phone1 = new PhoneNumber("+391234567890");
        var phone2 = new PhoneNumber("+391234567891");

        // Act & Assert
        Assert.False(phone1.Equals(phone2));
        Assert.True(phone1 != phone2);
    }

    [Fact]
    public void Equals_WithNull_ReturnsFalse()
    {
        // Arrange
        var phone = new PhoneNumber("+391234567890");

        // Act & Assert
        Assert.False(phone.Equals(null));
    }

    // ===== CONVERSION TESTS =====

    [Fact]
    public void ToString_ReturnsValue()
    {
        // Arrange
        var phone = new PhoneNumber("+391234567890");

        // Act & Assert
        Assert.Equal("+391234567890", phone.ToString());
    }

    [Fact]
    public void ImplicitConversion_ToString_Works()
    {
        // Arrange
        var phone = new PhoneNumber("+391234567890");

        // Act
        string value = phone;

        // Assert
        Assert.Equal("+391234567890", value);
    }
}
