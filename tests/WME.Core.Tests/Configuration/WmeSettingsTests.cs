namespace WME.Core.Tests.Configuration;

using Microsoft.Extensions.Logging;
using Moq;
using WME.Core.Configuration;
using Xunit;

/// <summary>
/// Tests for the WmeSettings class.
/// </summary>
public class WmeSettingsTests : IDisposable
{
    private readonly string _testConfigPath;

    public WmeSettingsTests()
    {
        _testConfigPath = Path.Combine(Path.GetTempPath(), $"wme_test_{Guid.NewGuid()}.json");
    }

    public void Dispose()
    {
        // Clean up test file
        if (File.Exists(_testConfigPath))
        {
            File.Delete(_testConfigPath);
        }
    }

    [Fact]
    public void Constructor_ShouldLoadDefaults()
    {
        // Arrange
        var logger = new Mock<ILogger<WmeSettings>>();

        // Act
        using var settings = new WmeSettings(logger.Object, _testConfigPath);

        // Assert
        Assert.Equal(1920, settings.ReadInt("Video", "Width"));
        Assert.Equal(1080, settings.ReadInt("Video", "Height"));
        Assert.True(settings.ReadBool("Video", "Windowed"));
    }

    [Fact]
    public void ReadString_ShouldReturnDefaultForMissingKey()
    {
        // Arrange
        var logger = new Mock<ILogger<WmeSettings>>();
        using var settings = new WmeSettings(logger.Object, _testConfigPath);

        // Act
        var value = settings.ReadString("Test", "Missing", "default");

        // Assert
        Assert.Equal("default", value);
    }

    [Fact]
    public void WriteString_ShouldStoreValue()
    {
        // Arrange
        var logger = new Mock<ILogger<WmeSettings>>();
        using var settings = new WmeSettings(logger.Object, _testConfigPath);

        // Act
        var result = settings.WriteString("Test", "Key", "Value");

        // Assert
        Assert.True(result);
        Assert.Equal("Value", settings.ReadString("Test", "Key"));
    }

    [Fact]
    public void ReadInt_ShouldReturnDefaultForMissingKey()
    {
        // Arrange
        var logger = new Mock<ILogger<WmeSettings>>();
        using var settings = new WmeSettings(logger.Object, _testConfigPath);

        // Act
        var value = settings.ReadInt("Test", "Missing", 999);

        // Assert
        Assert.Equal(999, value);
    }

    [Fact]
    public void WriteInt_ShouldStoreValue()
    {
        // Arrange
        var logger = new Mock<ILogger<WmeSettings>>();
        using var settings = new WmeSettings(logger.Object, _testConfigPath);

        // Act
        var result = settings.WriteInt("Test", "IntKey", 42);

        // Assert
        Assert.True(result);
        Assert.Equal(42, settings.ReadInt("Test", "IntKey"));
    }

    [Fact]
    public void ReadBool_ShouldReturnDefaultForMissingKey()
    {
        // Arrange
        var logger = new Mock<ILogger<WmeSettings>>();
        using var settings = new WmeSettings(logger.Object, _testConfigPath);

        // Act
        var value = settings.ReadBool("Test", "Missing", true);

        // Assert
        Assert.True(value);
    }

    [Fact]
    public void WriteBool_ShouldStoreValue()
    {
        // Arrange
        var logger = new Mock<ILogger<WmeSettings>>();
        using var settings = new WmeSettings(logger.Object, _testConfigPath);

        // Act
        var result = settings.WriteBool("Test", "BoolKey", false);

        // Assert
        Assert.True(result);
        Assert.False(settings.ReadBool("Test", "BoolKey"));
    }

    [Fact]
    public void ReadFloat_ShouldReturnDefaultForMissingKey()
    {
        // Arrange
        var logger = new Mock<ILogger<WmeSettings>>();
        using var settings = new WmeSettings(logger.Object, _testConfigPath);

        // Act
        var value = settings.ReadFloat("Test", "Missing", 3.14f);

        // Assert
        Assert.Equal(3.14f, value, precision: 2);
    }

    [Fact]
    public void WriteFloat_ShouldStoreValue()
    {
        // Arrange
        var logger = new Mock<ILogger<WmeSettings>>();
        using var settings = new WmeSettings(logger.Object, _testConfigPath);

        // Act
        var result = settings.WriteFloat("Test", "FloatKey", 2.718f);

        // Assert
        Assert.True(result);
        Assert.Equal(2.718f, settings.ReadFloat("Test", "FloatKey"), precision: 3);
    }

    [Fact]
    public void HasKey_ShouldReturnTrueForExistingKey()
    {
        // Arrange
        var logger = new Mock<ILogger<WmeSettings>>();
        using var settings = new WmeSettings(logger.Object, _testConfigPath);
        settings.WriteString("Test", "Key", "Value");

        // Act
        var exists = settings.HasKey("Test", "Key");

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public void HasKey_ShouldReturnFalseForMissingKey()
    {
        // Arrange
        var logger = new Mock<ILogger<WmeSettings>>();
        using var settings = new WmeSettings(logger.Object, _testConfigPath);

        // Act
        var exists = settings.HasKey("Test", "Missing");

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public void RemoveKey_ShouldRemoveExistingKey()
    {
        // Arrange
        var logger = new Mock<ILogger<WmeSettings>>();
        using var settings = new WmeSettings(logger.Object, _testConfigPath);
        settings.WriteString("Test", "Key", "Value");

        // Act
        var result = settings.RemoveKey("Test", "Key");

        // Assert
        Assert.True(result);
        Assert.False(settings.HasKey("Test", "Key"));
    }

    [Fact]
    public void RemoveKey_ShouldReturnFalseForMissingKey()
    {
        // Arrange
        var logger = new Mock<ILogger<WmeSettings>>();
        using var settings = new WmeSettings(logger.Object, _testConfigPath);

        // Act
        var result = settings.RemoveKey("Test", "Missing");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Save_ShouldPersistSettings()
    {
        // Arrange
        var logger = new Mock<ILogger<WmeSettings>>();

        // Act
        using (var settings = new WmeSettings(logger.Object, _testConfigPath))
        {
            settings.WriteString("Test", "Key", "Value");
            settings.WriteInt("Test", "Number", 123);
            settings.Save();
        }

        // Assert - reload and verify
        using (var settings = new WmeSettings(logger.Object, _testConfigPath))
        {
            Assert.Equal("Value", settings.ReadString("Test", "Key"));
            Assert.Equal(123, settings.ReadInt("Test", "Number"));
        }
    }

    [Fact]
    public void Load_ShouldLoadExistingFile()
    {
        // Arrange
        var logger = new Mock<ILogger<WmeSettings>>();

        // Create initial settings
        using (var settings = new WmeSettings(logger.Object, _testConfigPath))
        {
            settings.WriteString("Test", "Key", "InitialValue");
            settings.Save();
        }

        // Act - load in new instance
        using var newSettings = new WmeSettings(logger.Object, _testConfigPath);

        // Assert
        Assert.Equal("InitialValue", newSettings.ReadString("Test", "Key"));
    }

    [Fact]
    public void Clear_ShouldResetToDefaults()
    {
        // Arrange
        var logger = new Mock<ILogger<WmeSettings>>();
        using var settings = new WmeSettings(logger.Object, _testConfigPath);
        settings.WriteString("Test", "Key", "Value");

        // Act
        settings.Clear();

        // Assert
        Assert.False(settings.HasKey("Test", "Key"));
        Assert.Equal(1920, settings.ReadInt("Video", "Width")); // Default restored
    }

    [Fact]
    public void Dispose_ShouldAutoSave()
    {
        // Arrange
        var logger = new Mock<ILogger<WmeSettings>>();

        // Act
        using (var settings = new WmeSettings(logger.Object, _testConfigPath))
        {
            settings.WriteString("Test", "Key", "AutoSaveValue");
        } // Dispose should auto-save

        // Assert - reload and verify
        using var newSettings = new WmeSettings(logger.Object, _testConfigPath);
        Assert.Equal("AutoSaveValue", newSettings.ReadString("Test", "Key"));
    }

    [Fact]
    public void Settings_ShouldBeCaseInsensitive()
    {
        // Arrange
        var logger = new Mock<ILogger<WmeSettings>>();
        using var settings = new WmeSettings(logger.Object, _testConfigPath);
        settings.WriteString("Test", "Key", "Value");

        // Act & Assert
        Assert.Equal("Value", settings.ReadString("test", "key"));
        Assert.Equal("Value", settings.ReadString("TEST", "KEY"));
        Assert.Equal("Value", settings.ReadString("Test", "Key"));
    }
}
