using NotificationService.Domain.ValueObjects;

namespace NotificationService.Domain.Tests.ValueObjects;

public class TemplateDataTests
{
    // ===== CONSTRUCTOR TESTS =====

    [Fact]
    public void Constructor_WithValidData_CreatesTemplateData()
    {
        // Arrange
        var values = new Dictionary<string, string>
        {
            ["userName"] = "Mario",
            ["orderId"] = "12345"
        };

        // Act
        var data = new TemplateData(values);

        // Assert
        Assert.Equal("Mario", data.GetValue("userName"));
        Assert.Equal("12345", data.GetValue("orderId"));
    }

    [Fact]
    public void Constructor_WithNullDictionary_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new TemplateData(null!));
    }

    [Fact]
    public void Constructor_WithEmptyDictionary_CreatesEmptyTemplateData()
    {
        // Arrange
        var values = new Dictionary<string, string>();

        // Act
        var data = new TemplateData(values);

        // Assert
        Assert.Empty(data.Keys);
    }

    [Fact]
    public void Constructor_CreatesDefensiveCopy()
    {
        // Arrange
        var values = new Dictionary<string, string> { ["key"] = "original" };
        var data = new TemplateData(values);

        // Act - modifica il dictionary originale
        values["key"] = "modified";

        // Assert - TemplateData non deve essere modificato
        Assert.Equal("original", data.GetValue("key"));
    }

    // ===== GETVALUE TESTS =====

    [Fact]
    public void GetValue_WithExistingKey_ReturnsValue()
    {
        // Arrange
        var data = new TemplateData(new Dictionary<string, string> { ["name"] = "Test" });

        // Act
        var value = data.GetValue("name");

        // Assert
        Assert.Equal("Test", value);
    }

    [Fact]
    public void GetValue_WithMissingKey_ThrowsKeyNotFoundException()
    {
        // Arrange
        var data = new TemplateData(new Dictionary<string, string> { ["name"] = "Test" });

        // Act & Assert
        var exception = Assert.Throws<KeyNotFoundException>(() => data.GetValue("missing"));
        Assert.Contains("missing", exception.Message);
    }

    // ===== HASVALUE TESTS =====

    [Fact]
    public void HasValue_WithExistingKey_ReturnsTrue()
    {
        // Arrange
        var data = new TemplateData(new Dictionary<string, string> { ["name"] = "Test" });

        // Act & Assert
        Assert.True(data.HasValue("name"));
    }

    [Fact]
    public void HasValue_WithMissingKey_ReturnsFalse()
    {
        // Arrange
        var data = new TemplateData(new Dictionary<string, string> { ["name"] = "Test" });

        // Act & Assert
        Assert.False(data.HasValue("missing"));
    }

    // ===== KEYS TESTS =====

    [Fact]
    public void Keys_ReturnsAllKeys()
    {
        // Arrange
        var data = new TemplateData(new Dictionary<string, string>
        {
            ["a"] = "1",
            ["b"] = "2",
            ["c"] = "3"
        });

        // Act
        var keys = data.Keys;

        // Assert
        Assert.Equal(3, keys.Count);
        Assert.Contains("a", keys);
        Assert.Contains("b", keys);
        Assert.Contains("c", keys);
    }

    // ===== EMPTY TESTS =====

    [Fact]
    public void Empty_ReturnsEmptyTemplateData()
    {
        // Act
        var data = TemplateData.Empty;

        // Assert
        Assert.Empty(data.Keys);
    }

    // ===== EQUALITY TESTS =====

    [Fact]
    public void Equals_WithSameValues_ReturnsTrue()
    {
        // Arrange
        var data1 = new TemplateData(new Dictionary<string, string> { ["name"] = "Test" });
        var data2 = new TemplateData(new Dictionary<string, string> { ["name"] = "Test" });

        // Act & Assert
        Assert.True(data1.Equals(data2));
        Assert.True(data1 == data2);
    }

    [Fact]
    public void Equals_WithDifferentValues_ReturnsFalse()
    {
        // Arrange
        var data1 = new TemplateData(new Dictionary<string, string> { ["name"] = "Test1" });
        var data2 = new TemplateData(new Dictionary<string, string> { ["name"] = "Test2" });

        // Act & Assert
        Assert.False(data1.Equals(data2));
        Assert.True(data1 != data2);
    }

    [Fact]
    public void Equals_WithDifferentKeys_ReturnsFalse()
    {
        // Arrange
        var data1 = new TemplateData(new Dictionary<string, string> { ["name"] = "Test" });
        var data2 = new TemplateData(new Dictionary<string, string> { ["other"] = "Test" });

        // Act & Assert
        Assert.False(data1.Equals(data2));
    }

    [Fact]
    public void Equals_WithNull_ReturnsFalse()
    {
        // Arrange
        var data = new TemplateData(new Dictionary<string, string> { ["name"] = "Test" });

        // Act & Assert
        Assert.False(data.Equals(null));
    }

    [Fact]
    public void GetHashCode_WithSameValues_ReturnsSameHash()
    {
        // Arrange
        var data1 = new TemplateData(new Dictionary<string, string> { ["name"] = "Test" });
        var data2 = new TemplateData(new Dictionary<string, string> { ["name"] = "Test" });

        // Act & Assert
        Assert.Equal(data1.GetHashCode(), data2.GetHashCode());
    }
}
