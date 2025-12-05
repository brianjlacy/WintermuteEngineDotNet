namespace WME.Core.Files;

/// <summary>
/// File management interface for accessing game assets.
/// Supports both regular file system and compressed packages (DCP/ZIP).
/// </summary>
public interface IWmeFileManager : IDisposable
{
    /// <summary>
    /// Opens a file for reading.
    /// </summary>
    /// <param name="filename">Path to the file (relative to game directory or package).</param>
    /// <returns>A stream for reading the file, or null if file not found.</returns>
    Stream? OpenFile(string filename);

    /// <summary>
    /// Opens a file for reading asynchronously.
    /// </summary>
    /// <param name="filename">Path to the file.</param>
    /// <returns>A stream for reading the file, or null if file not found.</returns>
    Task<Stream?> OpenFileAsync(string filename);

    /// <summary>
    /// Checks if a file exists.
    /// </summary>
    /// <param name="filename">Path to the file.</param>
    /// <returns>True if the file exists.</returns>
    bool FileExists(string filename);

    /// <summary>
    /// Gets the size of a file in bytes.
    /// </summary>
    /// <param name="filename">Path to the file.</param>
    /// <returns>File size in bytes, or -1 if file not found.</returns>
    long GetFileSize(string filename);

    /// <summary>
    /// Registers a package file (DCP or ZIP) to be searched for assets.
    /// </summary>
    /// <param name="packagePath">Path to the package file.</param>
    /// <param name="priority">Search priority (higher = searched first).</param>
    /// <returns>True if the package was registered successfully.</returns>
    Task<bool> RegisterPackageAsync(string packagePath, int priority = 0);

    /// <summary>
    /// Unregisters a previously registered package.
    /// </summary>
    /// <param name="packagePath">Path to the package file.</param>
    void UnregisterPackage(string packagePath);

    /// <summary>
    /// Lists all files matching a pattern.
    /// </summary>
    /// <param name="pattern">File pattern (supports wildcards).</param>
    /// <param name="recursive">If true, search subdirectories.</param>
    /// <returns>List of matching file paths.</returns>
    IEnumerable<string> FindFiles(string pattern, bool recursive = false);

    /// <summary>
    /// Reads an entire file into a byte array.
    /// </summary>
    /// <param name="filename">Path to the file.</param>
    /// <returns>File contents as byte array, or null if file not found.</returns>
    Task<byte[]?> ReadAllBytesAsync(string filename);

    /// <summary>
    /// Reads an entire text file.
    /// </summary>
    /// <param name="filename">Path to the file.</param>
    /// <returns>File contents as string, or null if file not found.</returns>
    Task<string?> ReadAllTextAsync(string filename);
}
