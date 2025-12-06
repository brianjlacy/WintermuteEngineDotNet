namespace WME.Core.Files;

using System.IO.Compression;
using System.Text;

/// <summary>
/// Reader for Wintermute Engine DCP (Dead:Code Package) files.
/// DCP is a proprietary package format used by the original Wintermute Engine.
/// </summary>
public class DcpPackageReader : IPackageReader
{
    private readonly ILogger _logger;
    private readonly Dictionary<string, DcpEntry> _entries = new(StringComparer.OrdinalIgnoreCase);
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
    /// Initializes a new instance of the <see cref="DcpPackageReader"/> class.
    /// </summary>
    public DcpPackageReader(ILogger logger, string filePath, int priority = 0)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        Name = Path.GetFileName(filePath);
        Priority = priority;
    }

    /// <summary>
    /// Initializes the package by reading the file table.
    /// </summary>
    public async Task InitializeAsync()
    {
        ThrowIfDisposed();

        try
        {
            _fileStream = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = new BinaryReader(_fileStream, Encoding.UTF8, leaveOpen: true);

            // Read DCP header
            var magic = reader.ReadUInt32();
            if (magic != 0x44435031) // "DCP1" in little-endian
            {
                throw new InvalidDataException($"Invalid DCP magic number: 0x{magic:X8}");
            }

            var version = reader.ReadUInt32();
            _logger.LogDebug("DCP version: {Version}", version);

            var tableOffset = reader.ReadUInt32();
            var tableCompSize = reader.ReadUInt32();
            var tableUncompSize = reader.ReadUInt32();

            // Read file table
            _fileStream.Seek(tableOffset, SeekOrigin.Begin);
            var compressedTable = reader.ReadBytes((int)tableCompSize);

            byte[] tableData;
            if (tableCompSize < tableUncompSize)
            {
                // Table is compressed
                tableData = await DecompressZlibAsync(compressedTable, (int)tableUncompSize);
            }
            else
            {
                tableData = compressedTable;
            }

            // Parse file table
            using var tableStream = new MemoryStream(tableData);
            using var tableReader = new BinaryReader(tableStream, Encoding.UTF8);

            var fileCount = tableReader.ReadUInt32();
            _logger.LogInformation("DCP contains {FileCount} files", fileCount);

            for (uint i = 0; i < fileCount; i++)
            {
                var entry = new DcpEntry
                {
                    NameLength = tableReader.ReadUInt32(),
                };

                entry.Name = Encoding.UTF8.GetString(tableReader.ReadBytes((int)entry.NameLength));
                entry.Offset = tableReader.ReadUInt32();
                entry.CompSize = tableReader.ReadUInt32();
                entry.UncompSize = tableReader.ReadUInt32();
                entry.Flags = tableReader.ReadUInt32();

                _entries[NormalizePath(entry.Name)] = entry;
            }

            _logger.LogInformation("Loaded DCP package: {Name} ({FileCount} files)", Name, fileCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize DCP package: {Path}", FilePath);
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

        filename = NormalizePath(filename);

        if (!_entries.TryGetValue(filename, out var entry))
            return null;

        try
        {
            lock (_fileStream!)
            {
                _fileStream.Seek(entry.Offset, SeekOrigin.Begin);
                var data = new byte[entry.CompSize];
                _fileStream.Read(data, 0, data.Length);

                if (entry.CompSize < entry.UncompSize)
                {
                    // File is compressed
                    var decompressed = DecompressZlibAsync(data, (int)entry.UncompSize).Result;
                    return new MemoryStream(decompressed, writable: false);
                }
                else
                {
                    return new MemoryStream(data, writable: false);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open file from DCP: {File}", filename);
            return null;
        }
    }

    /// <summary>
    /// Opens a file from the package asynchronously.
    /// </summary>
    public async Task<Stream?> OpenFileAsync(string filename)
    {
        ThrowIfDisposed();

        filename = NormalizePath(filename);

        if (!_entries.TryGetValue(filename, out var entry))
            return null;

        try
        {
            byte[] data;
            lock (_fileStream!)
            {
                _fileStream.Seek(entry.Offset, SeekOrigin.Begin);
                data = new byte[entry.CompSize];
                _fileStream.Read(data, 0, data.Length);
            }

            if (entry.CompSize < entry.UncompSize)
            {
                var decompressed = await DecompressZlibAsync(data, (int)entry.UncompSize);
                return new MemoryStream(decompressed, writable: false);
            }
            else
            {
                return new MemoryStream(data, writable: false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open file from DCP: {File}", filename);
            return null;
        }
    }

    /// <summary>
    /// Checks if a file exists in the package.
    /// </summary>
    public bool FileExists(string filename)
    {
        ThrowIfDisposed();
        return _entries.ContainsKey(NormalizePath(filename));
    }

    /// <summary>
    /// Gets the size of a file in the package.
    /// </summary>
    public long GetFileSize(string filename)
    {
        ThrowIfDisposed();

        if (_entries.TryGetValue(NormalizePath(filename), out var entry))
            return entry.UncompSize;

        return -1;
    }

    /// <summary>
    /// Gets all files matching a pattern.
    /// </summary>
    public IEnumerable<string> GetFiles(string pattern, bool recursive)
    {
        ThrowIfDisposed();

        // Simple wildcard matching
        var regex = "^" + System.Text.RegularExpressions.Regex.Escape(pattern)
            .Replace("\\*", ".*")
            .Replace("\\?", ".") + "$";

        var regexObj = new System.Text.RegularExpressions.Regex(regex,
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        foreach (var entry in _entries.Keys)
        {
            if (!recursive && entry.Contains('/'))
                continue;

            if (regexObj.IsMatch(Path.GetFileName(entry)))
                yield return entry;
        }
    }

    /// <summary>
    /// Decompresses zlib-compressed data.
    /// </summary>
    private static async Task<byte[]> DecompressZlibAsync(byte[] compressedData, int uncompressedSize)
    {
        // Skip zlib header (2 bytes)
        using var inputStream = new MemoryStream(compressedData, 2, compressedData.Length - 2);
        using var deflateStream = new DeflateStream(inputStream, CompressionMode.Decompress);
        using var outputStream = new MemoryStream(uncompressedSize);

        await deflateStream.CopyToAsync(outputStream);
        return outputStream.ToArray();
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
            throw new ObjectDisposedException(nameof(DcpPackageReader));
    }

    /// <summary>
    /// Disposes the package reader.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        _fileStream?.Dispose();
        _fileStream = null;
        _entries.Clear();

        _disposed = true;
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Represents a file entry in a DCP package.
    /// </summary>
    private class DcpEntry
    {
        public uint NameLength { get; set; }
        public string Name { get; set; } = string.Empty;
        public uint Offset { get; set; }
        public uint CompSize { get; set; }
        public uint UncompSize { get; set; }
        public uint Flags { get; set; }
    }
}
