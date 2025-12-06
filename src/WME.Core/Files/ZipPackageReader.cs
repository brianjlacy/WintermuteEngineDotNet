namespace WME.Core.Files;

using System.IO.Compression;

/// <summary>
/// Reader for ZIP archive packages.
/// </summary>
public class ZipPackageReader : IPackageReader
{
    private readonly ILogger _logger;
    private ZipArchive? _archive;
    private FileStream? _fileStream;
    private bool _disposed;

    /// <summary>
    /// Gets the package file path.
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    /// Gets the package name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the search priority.
    /// </summary>
    public int Priority { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ZipPackageReader"/> class.
    /// </summary>
    public ZipPackageReader(ILogger logger, string filePath, int priority = 0)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        Name = Path.GetFileName(filePath);
        Priority = priority;
    }

    /// <summary>
    /// Initializes the package by opening the ZIP archive.
    /// </summary>
    public Task InitializeAsync()
    {
        ThrowIfDisposed();

        try
        {
            _fileStream = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            _archive = new ZipArchive(_fileStream, ZipArchiveMode.Read, leaveOpen: false);

            _logger.LogInformation("Loaded ZIP package: {Name} ({EntryCount} files)", Name, _archive.Entries.Count);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize ZIP package: {Path}", FilePath);
            _fileStream?.Dispose();
            _fileStream = null;
            throw;
        }
    }

    /// <summary>
    /// Opens a file from the package.
    /// </summary>
    public Stream? OpenFile(string filename)
    {
        ThrowIfDisposed();

        if (_archive == null)
            return null;

        filename = NormalizePath(filename);

        var entry = _archive.GetEntry(filename);
        if (entry == null)
            return null;

        try
        {
            // Read the entire entry into a MemoryStream so we can return it
            // (ZipArchive entries can't be opened multiple times concurrently)
            var stream = entry.Open();
            var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            stream.Dispose();
            memoryStream.Position = 0;
            return memoryStream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open file from ZIP: {File}", filename);
            return null;
        }
    }

    /// <summary>
    /// Opens a file from the package asynchronously.
    /// </summary>
    public async Task<Stream?> OpenFileAsync(string filename)
    {
        ThrowIfDisposed();

        if (_archive == null)
            return null;

        filename = NormalizePath(filename);

        var entry = _archive.GetEntry(filename);
        if (entry == null)
            return null;

        try
        {
            var stream = entry.Open();
            var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            stream.Dispose();
            memoryStream.Position = 0;
            return memoryStream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open file from ZIP: {File}", filename);
            return null;
        }
    }

    /// <summary>
    /// Checks if a file exists in the package.
    /// </summary>
    public bool FileExists(string filename)
    {
        ThrowIfDisposed();

        if (_archive == null)
            return false;

        filename = NormalizePath(filename);
        return _archive.GetEntry(filename) != null;
    }

    /// <summary>
    /// Gets the size of a file in the package.
    /// </summary>
    public long GetFileSize(string filename)
    {
        ThrowIfDisposed();

        if (_archive == null)
            return -1;

        filename = NormalizePath(filename);
        var entry = _archive.GetEntry(filename);
        return entry?.Length ?? -1;
    }

    /// <summary>
    /// Gets all files matching a pattern.
    /// </summary>
    public IEnumerable<string> GetFiles(string pattern, bool recursive)
    {
        ThrowIfDisposed();

        if (_archive == null)
            yield break;

        var regex = "^" + System.Text.RegularExpressions.Regex.Escape(pattern)
            .Replace("\\*", ".*")
            .Replace("\\?", ".") + "$";

        var regexObj = new System.Text.RegularExpressions.Regex(regex,
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        foreach (var entry in _archive.Entries)
        {
            var normalizedName = NormalizePath(entry.FullName);

            if (!recursive && normalizedName.Contains('/'))
                continue;

            if (regexObj.IsMatch(Path.GetFileName(normalizedName)))
                yield return normalizedName;
        }
    }

    /// <summary>
    /// Normalizes path separators.
    /// </summary>
    private static string NormalizePath(string path)
    {
        return path.Replace('\\', '/');
    }

    /// <summary>
    /// Throws if disposed.
    /// </summary>
    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ZipPackageReader));
    }

    /// <summary>
    /// Disposes the package reader.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        _archive?.Dispose();
        _archive = null;
        _fileStream?.Dispose();
        _fileStream = null;

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
