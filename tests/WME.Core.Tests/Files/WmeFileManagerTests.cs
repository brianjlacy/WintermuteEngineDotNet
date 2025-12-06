namespace WME.Core.Tests.Files;

using Microsoft.Extensions.Logging;
using Moq;
using WME.Core.Files;
using Xunit;

/// <summary>
/// Tests for the WmeFileManager class.
/// </summary>
public class WmeFileManagerTests : IDisposable
{
    private readonly string _testDirectory;

    public WmeFileManagerTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"wme_filetest_{Guid.NewGuid()}");
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
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new WmeFileManager(null!, _testDirectory));
    }

    [Fact]
    public void Constructor_ShouldRequireGameDirectory()
    {
        // Arrange
        var logger = new Mock<ILogger<WmeFileManager>>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new WmeFileManager(logger.Object, null!));
    }

    [Fact]
    public void OpenFile_ShouldReturnNullForMissingFile()
    {
        // Arrange
        var logger = new Mock<ILogger<WmeFileManager>>();
        using var fileManager = new WmeFileManager(logger.Object, _testDirectory);

        // Act
        var stream = fileManager.OpenFile("missing.txt");

        // Assert
        Assert.Null(stream);
    }

    [Fact]
    public void OpenFile_ShouldOpenExistingFile()
    {
        // Arrange
        var logger = new Mock<ILogger<WmeFileManager>>();
        var testFile = Path.Combine(_testDirectory, "test.txt");
        File.WriteAllText(testFile, "Hello World");

        using var fileManager = new WmeFileManager(logger.Object, _testDirectory);

        // Act
        using var stream = fileManager.OpenFile("test.txt");

        // Assert
        Assert.NotNull(stream);
        using var reader = new StreamReader(stream!);
        var content = reader.ReadToEnd();
        Assert.Equal("Hello World", content);
    }

    [Fact]
    public async Task OpenFileAsync_ShouldOpenExistingFile()
    {
        // Arrange
        var logger = new Mock<ILogger<WmeFileManager>>();
        var testFile = Path.Combine(_testDirectory, "test.txt");
        File.WriteAllText(testFile, "Async Content");

        using var fileManager = new WmeFileManager(logger.Object, _testDirectory);

        // Act
        using var stream = await fileManager.OpenFileAsync("test.txt");

        // Assert
        Assert.NotNull(stream);
        using var reader = new StreamReader(stream!);
        var content = await reader.ReadToEndAsync();
        Assert.Equal("Async Content", content);
    }

    [Fact]
    public void FileExists_ShouldReturnTrueForExistingFile()
    {
        // Arrange
        var logger = new Mock<ILogger<WmeFileManager>>();
        var testFile = Path.Combine(_testDirectory, "exists.txt");
        File.WriteAllText(testFile, "Content");

        using var fileManager = new WmeFileManager(logger.Object, _testDirectory);

        // Act
        var exists = fileManager.FileExists("exists.txt");

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public void FileExists_ShouldReturnFalseForMissingFile()
    {
        // Arrange
        var logger = new Mock<ILogger<WmeFileManager>>();
        using var fileManager = new WmeFileManager(logger.Object, _testDirectory);

        // Act
        var exists = fileManager.FileExists("missing.txt");

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public void GetFileSize_ShouldReturnCorrectSize()
    {
        // Arrange
        var logger = new Mock<ILogger<WmeFileManager>>();
        var testFile = Path.Combine(_testDirectory, "sized.txt");
        var content = "This is exactly 25 bytes";
        File.WriteAllText(testFile, content);

        using var fileManager = new WmeFileManager(logger.Object, _testDirectory);

        // Act
        var size = fileManager.GetFileSize("sized.txt");

        // Assert
        Assert.Equal(content.Length, size);
    }

    [Fact]
    public void GetFileSize_ShouldReturnNegativeForMissingFile()
    {
        // Arrange
        var logger = new Mock<ILogger<WmeFileManager>>();
        using var fileManager = new WmeFileManager(logger.Object, _testDirectory);

        // Act
        var size = fileManager.GetFileSize("missing.txt");

        // Assert
        Assert.True(size < 0);
    }

    [Fact]
    public async Task ReadAllBytesAsync_ShouldReadFile()
    {
        // Arrange
        var logger = new Mock<ILogger<WmeFileManager>>();
        var testFile = Path.Combine(_testDirectory, "bytes.bin");
        var testData = new byte[] { 1, 2, 3, 4, 5 };
        File.WriteAllBytes(testFile, testData);

        using var fileManager = new WmeFileManager(logger.Object, _testDirectory);

        // Act
        var data = await fileManager.ReadAllBytesAsync("bytes.bin");

        // Assert
        Assert.NotNull(data);
        Assert.Equal(testData, data);
    }

    [Fact]
    public async Task ReadAllTextAsync_ShouldReadTextFile()
    {
        // Arrange
        var logger = new Mock<ILogger<WmeFileManager>>();
        var testFile = Path.Combine(_testDirectory, "text.txt");
        var testText = "UTF-8 Text Content";
        File.WriteAllText(testFile, testText);

        using var fileManager = new WmeFileManager(logger.Object, _testDirectory);

        // Act
        var text = await fileManager.ReadAllTextAsync("text.txt");

        // Assert
        Assert.Equal(testText, text);
    }

    [Fact]
    public void FindFiles_ShouldFindMatchingFiles()
    {
        // Arrange
        var logger = new Mock<ILogger<WmeFileManager>>();
        File.WriteAllText(Path.Combine(_testDirectory, "file1.txt"), "");
        File.WriteAllText(Path.Combine(_testDirectory, "file2.txt"), "");
        File.WriteAllText(Path.Combine(_testDirectory, "other.dat"), "");

        using var fileManager = new WmeFileManager(logger.Object, _testDirectory);

        // Act
        var files = fileManager.FindFiles("*.txt", recursive: false).ToList();

        // Assert
        Assert.Equal(2, files.Count);
        Assert.Contains(files, f => f.Contains("file1.txt"));
        Assert.Contains(files, f => f.Contains("file2.txt"));
    }

    [Fact]
    public void OpenFile_ShouldNormalizePathSeparators()
    {
        // Arrange
        var logger = new Mock<ILogger<WmeFileManager>>();
        var subDir = Path.Combine(_testDirectory, "sub");
        Directory.CreateDirectory(subDir);
        var testFile = Path.Combine(subDir, "test.txt");
        File.WriteAllText(testFile, "Normalized");

        using var fileManager = new WmeFileManager(logger.Object, _testDirectory);

        // Act - use Windows-style path
        using var stream = fileManager.OpenFile("sub\\test.txt");

        // Assert
        Assert.NotNull(stream);
    }

    [Fact]
    public void Dispose_ShouldBeIdempotent()
    {
        // Arrange
        var logger = new Mock<ILogger<WmeFileManager>>();
        var fileManager = new WmeFileManager(logger.Object, _testDirectory);

        // Act
        fileManager.Dispose();
        fileManager.Dispose();

        // Assert - no exception thrown
        Assert.True(true);
    }

    [Fact]
    public void OpenFile_ShouldThrowWhenDisposed()
    {
        // Arrange
        var logger = new Mock<ILogger<WmeFileManager>>();
        var fileManager = new WmeFileManager(logger.Object, _testDirectory);
        fileManager.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() =>
            fileManager.OpenFile("test.txt"));
    }
}
