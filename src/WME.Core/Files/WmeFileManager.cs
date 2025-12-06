namespace WME.Core.Files;

using System.Text;

/// <summary>
/// File manager implementation for accessing game assets from file system and packages.
/// </summary>
public class WmeFileManager : IWmeFileManager
{
    private readonly ILogger<WmeFileManager> _logger;
    private readonly List<IPackageReader> _packages = new();
    private readonly string _gameDirectory;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="WmeFileManager"/> class.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    /// <param name="gameDirectory">Root game directory path.</param>
    public WmeFileManager(ILogger<WmeFileManager> logger, string gameDirectory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _gameDirectory = gameDirectory ?? throw new ArgumentNullException(nameof(gameDirectory));

        if (!Directory.Exists(_gameDirectory))
        {
            _logger.LogWarning("Game directory does not exist: {Directory}", _gameDirectory);
        }
    }

    /// <summary>
    /// Opens a file for reading.
    /// </summary>
    public Stream? OpenFile(string filename)
    {
        ThrowIfDisposed();

        if (string.IsNullOrWhiteSpace(filename))
            throw new ArgumentException("Filename cannot be null or empty", nameof(filename));

        // Normalize path separators
        filename = NormalizePath(filename);

        // Search packages first (in priority order)
        var packages = _packages.OrderByDescending(p => p.Priority).ToList();
        foreach (var package in packages)
        {
            var stream = package.OpenFile(filename);
            if (stream != null)
            {
                _logger.LogDebug("Opened '{File}' from package '{Package}'", filename, package.Name);
                return stream;
            }
        }

        // Search file system
        var fullPath = Path.Combine(_gameDirectory, filename);
        if (File.Exists(fullPath))
        {
            _logger.LogDebug("Opened '{File}' from file system", filename);
            return File.OpenRead(fullPath);
        }

        _logger.LogWarning("File not found: {File}", filename);
        return null;
    }

    /// <summary>
    /// Opens a file for reading asynchronously.
    /// </summary>
    public async Task<Stream?> OpenFileAsync(string filename)
    {
        ThrowIfDisposed();

        if (string.IsNullOrWhiteSpace(filename))
            throw new ArgumentException("Filename cannot be null or empty", nameof(filename));

        filename = NormalizePath(filename);

        // Search packages
        var packages = _packages.OrderByDescending(p => p.Priority).ToList();
        foreach (var package in packages)
        {
            var stream = await package.OpenFileAsync(filename);
            if (stream != null)
            {
                _logger.LogDebug("Opened '{File}' from package '{Package}'", filename, package.Name);
                return stream;
            }
        }

        // Search file system
        var fullPath = Path.Combine(_gameDirectory, filename);
        if (File.Exists(fullPath))
        {
            _logger.LogDebug("Opened '{File}' from file system", filename);
            return File.OpenRead(fullPath);
        }

        _logger.LogWarning("File not found: {File}", filename);
        return null;
    }

    /// <summary>
    /// Checks if a file exists.
    /// </summary>
    public bool FileExists(string filename)
    {
        ThrowIfDisposed();

        if (string.IsNullOrWhiteSpace(filename))
            return false;

        filename = NormalizePath(filename);

        // Check packages
        foreach (var package in _packages)
        {
            if (package.FileExists(filename))
                return true;
        }

        // Check file system
        var fullPath = Path.Combine(_gameDirectory, filename);
        return File.Exists(fullPath);
    }

    /// <summary>
    /// Gets the size of a file in bytes.
    /// </summary>
    public long GetFileSize(string filename)
    {
        ThrowIfDisposed();

        if (string.IsNullOrWhiteSpace(filename))
            return -1;

        filename = NormalizePath(filename);

        // Check packages
        var packages = _packages.OrderByDescending(p => p.Priority).ToList();
        foreach (var package in packages)
        {
            var size = package.GetFileSize(filename);
            if (size >= 0)
                return size;
        }

        // Check file system
        var fullPath = Path.Combine(_gameDirectory, filename);
        if (File.Exists(fullPath))
        {
            return new FileInfo(fullPath).Length;
        }

        return -1;
    }

    /// <summary>
    /// Registers a package file to be searched for assets.
    /// </summary>
    public async Task<bool> RegisterPackageAsync(string packagePath, int priority = 0)
    {
        ThrowIfDisposed();

        if (string.IsNullOrWhiteSpace(packagePath))
            throw new ArgumentException("Package path cannot be null or empty", nameof(packagePath));

        var fullPath = Path.Combine(_gameDirectory, packagePath);
        if (!File.Exists(fullPath))
        {
            _logger.LogError("Package file not found: {Path}", packagePath);
            return false;
        }

        try
        {
            IPackageReader? package = null;
            var extension = Path.GetExtension(packagePath).ToLowerInvariant();

            if (extension == ".dcp")
            {
                package = new DcpPackageReader(_logger, fullPath, priority);
            }
            else if (extension == ".zip")
            {
                package = new ZipPackageReader(_logger, fullPath, priority);
            }
            else
            {
                _logger.LogError("Unsupported package format: {Extension}", extension);
                return false;
            }

            await package.InitializeAsync();
            _packages.Add(package);

            _logger.LogInformation("Registered package '{Package}' with priority {Priority}", packagePath, priority);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register package: {Path}", packagePath);
            return false;
        }
    }

    /// <summary>
    /// Unregisters a previously registered package.
    /// </summary>
    public void UnregisterPackage(string packagePath)
    {
        ThrowIfDisposed();

        var fullPath = Path.Combine(_gameDirectory, packagePath);
        var package = _packages.FirstOrDefault(p =>
            string.Equals(p.FilePath, fullPath, StringComparison.OrdinalIgnoreCase));

        if (package != null)
        {
            _packages.Remove(package);
            package.Dispose();
            _logger.LogInformation("Unregistered package: {Package}", packagePath);
        }
    }

    /// <summary>
    /// Lists all files matching a pattern.
    /// </summary>
    public IEnumerable<string> FindFiles(string pattern, bool recursive = false)
    {
        ThrowIfDisposed();

        var files = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Search packages
        foreach (var package in _packages)
        {
            foreach (var file in package.GetFiles(pattern, recursive))
            {
                files.Add(file);
            }
        }

        // Search file system
        try
        {
            var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var fsFiles = Directory.GetFiles(_gameDirectory, pattern, searchOption);

            foreach (var file in fsFiles)
            {
                var relativePath = Path.GetRelativePath(_gameDirectory, file);
                files.Add(NormalizePath(relativePath));
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error searching file system for pattern: {Pattern}", pattern);
        }

        return files;
    }

    /// <summary>
    /// Reads an entire file into a byte array.
    /// </summary>
    public async Task<byte[]?> ReadAllBytesAsync(string filename)
    {
        ThrowIfDisposed();

        var stream = await OpenFileAsync(filename);
        if (stream == null)
            return null;

        try
        {
            using (stream)
            {
                var buffer = new byte[stream.Length];
                await stream.ReadAsync(buffer, 0, buffer.Length);
                return buffer;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read file: {File}", filename);
            return null;
        }
    }

    /// <summary>
    /// Reads an entire text file.
    /// </summary>
    public async Task<string?> ReadAllTextAsync(string filename)
    {
        ThrowIfDisposed();

        var bytes = await ReadAllBytesAsync(filename);
        if (bytes == null)
            return null;

        try
        {
            // Try UTF-8 first, then fallback to system default
            return Encoding.UTF8.GetString(bytes);
        }
        catch
        {
            return Encoding.Default.GetString(bytes);
        }
    }

    /// <summary>
    /// Normalizes path separators to forward slashes.
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
            throw new ObjectDisposedException(nameof(WmeFileManager));
    }

    /// <summary>
    /// Disposes the file manager and all registered packages.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        foreach (var package in _packages)
        {
            package?.Dispose();
        }
        _packages.Clear();

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Interface for package readers (DCP, ZIP, etc.).
/// </summary>
public interface IPackageReader : IDisposable
{
    /// <summary>
    /// Gets the package file path.
    /// </summary>
    string FilePath { get; }

    /// <summary>
    /// Gets the package name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the search priority.
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Initializes the package reader.
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// Opens a file from the package.
    /// </summary>
    Stream? OpenFile(string filename);

    /// <summary>
    /// Opens a file from the package asynchronously.
    /// </summary>
    Task<Stream?> OpenFileAsync(string filename);

    /// <summary>
    /// Checks if a file exists in the package.
    /// </summary>
    bool FileExists(string filename);

    /// <summary>
    /// Gets the size of a file in the package.
    /// </summary>
    long GetFileSize(string filename);

    /// <summary>
    /// Gets all files matching a pattern.
    /// </summary>
    IEnumerable<string> GetFiles(string pattern, bool recursive);
}
