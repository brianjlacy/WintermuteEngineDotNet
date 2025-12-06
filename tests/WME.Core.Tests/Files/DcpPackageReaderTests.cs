namespace WME.Core.Tests.Files;

using Microsoft.Extensions.Logging;
using Moq;
using WME.Core.Files;
using Xunit;

/// <summary>
/// Tests for the DcpPackageReader class.
/// </summary>
public class DcpPackageReaderTests : IDisposable
{
    private readonly string _testDirectory;

    public DcpPackageReaderTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"wme_dcptest_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
    }

    public void Dispose()
    {
        // Clean up test directory
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, recursive: true);
        }
    }

    [Fact]
    public void Constructor_ShouldRequireLogger()
    {
        // Arrange
        var testFile = Path.Combine(_testDirectory, "test.dcp");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new DcpPackageReader(null!, testFile));
    }

    [Fact]
    public void Constructor_ShouldRequireFilePath()
    {
        // Arrange
        var logger = new Mock<ILogger>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new DcpPackageReader(logger.Object, null!));
    }

    [Fact]
    public void Constructor_ShouldSetProperties()
    {
        // Arrange
        var logger = new Mock<ILogger>();
        var testFile = Path.Combine(_testDirectory, "test.dcp");

        // Act
        using var reader = new DcpPackageReader(logger.Object, testFile, priority: 10);

        // Assert
        Assert.Equal(testFile, reader.FilePath);
        Assert.Equal("test.dcp", reader.Name);
        Assert.Equal(10, reader.Priority);
    }

    [Fact]
    public async Task InitializeAsync_ShouldThrowForMissingFile()
    {
        // Arrange
        var logger = new Mock<ILogger>();
        var testFile = Path.Combine(_testDirectory, "missing.dcp");
        using var reader = new DcpPackageReader(logger.Object, testFile);

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(() =>
            reader.InitializeAsync());
    }

    [Fact]
    public async Task InitializeAsync_ShouldThrowForInvalidMagic()
    {
        // Arrange
        var logger = new Mock<ILogger>();
        var testFile = Path.Combine(_testDirectory, "invalid.dcp");

        // Create file with invalid magic number
        using (var writer = new BinaryWriter(File.Create(testFile)))
        {
            writer.Write(0xDEADBEEF); // Invalid magic
        }

        using var reader = new DcpPackageReader(logger.Object, testFile);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidDataException>(() =>
            reader.InitializeAsync());
    }

    [Fact]
    public void FileExists_ShouldReturnFalseWhenNotInitialized()
    {
        // Arrange
        var logger = new Mock<ILogger>();
        var testFile = Path.Combine(_testDirectory, "test.dcp");
        using var reader = new DcpPackageReader(logger.Object, testFile);

        // Act
        var exists = reader.FileExists("any.txt");

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public void OpenFile_ShouldReturnNullWhenNotInitialized()
    {
        // Arrange
        var logger = new Mock<ILogger>();
        var testFile = Path.Combine(_testDirectory, "test.dcp");
        using var reader = new DcpPackageReader(logger.Object, testFile);

        // Act
        var stream = reader.OpenFile("any.txt");

        // Assert
        Assert.Null(stream);
    }

    [Fact]
    public void GetFileSize_ShouldReturnNegativeWhenNotInitialized()
    {
        // Arrange
        var logger = new Mock<ILogger>();
        var testFile = Path.Combine(_testDirectory, "test.dcp");
        using var reader = new DcpPackageReader(logger.Object, testFile);

        // Act
        var size = reader.GetFileSize("any.txt");

        // Assert
        Assert.True(size < 0);
    }

    [Fact]
    public void GetFiles_ShouldReturnEmptyWhenNotInitialized()
    {
        // Arrange
        var logger = new Mock<ILogger>();
        var testFile = Path.Combine(_testDirectory, "test.dcp");
        using var reader = new DcpPackageReader(logger.Object, testFile);

        // Act
        var files = reader.GetFiles("*.*", recursive: true).ToList();

        // Assert
        Assert.Empty(files);
    }

    [Fact]
    public void Dispose_ShouldBeIdempotent()
    {
        // Arrange
        var logger = new Mock<ILogger>();
        var testFile = Path.Combine(_testDirectory, "test.dcp");
        var reader = new DcpPackageReader(logger.Object, testFile);

        // Act
        reader.Dispose();
        reader.Dispose();

        // Assert - no exception thrown
        Assert.True(true);
    }

    [Fact]
    public void FileExists_ShouldThrowWhenDisposed()
    {
        // Arrange
        var logger = new Mock<ILogger>();
        var testFile = Path.Combine(_testDirectory, "test.dcp");
        var reader = new DcpPackageReader(logger.Object, testFile);
        reader.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() =>
            reader.FileExists("any.txt"));
    }

    // Note: Full integration tests with valid DCP files would require
    // creating properly formatted DCP packages with compression.
    // These would be better suited for integration tests with sample assets.
}
