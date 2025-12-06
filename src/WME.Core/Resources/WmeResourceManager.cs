namespace WME.Core.Resources;

using System.Collections.Concurrent;
using WME.Core.Files;

/// <summary>
/// Resource manager implementation for caching and lifecycle management of game assets.
/// </summary>
public class WmeResourceManager : IWmeResourceManager
{
    private readonly ILogger<WmeResourceManager> _logger;
    private readonly IWmeFileManager _fileManager;
    private readonly ConcurrentDictionary<string, CachedResource> _cache = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<Type, IResourceLoader> _loaders = new();
    private bool _disposed;

    /// <summary>
    /// Gets the total number of cached resources.
    /// </summary>
    public int CachedResourceCount => _cache.Count;

    /// <summary>
    /// Gets the approximate memory usage of cached resources in bytes.
    /// </summary>
    public long CachedMemoryUsage
    {
        get
        {
            return _cache.Values.Sum(r => r.EstimatedSize);
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WmeResourceManager"/> class.
    /// </summary>
    public WmeResourceManager(ILogger<WmeResourceManager> logger, IWmeFileManager fileManager)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _fileManager = fileManager ?? throw new ArgumentNullException(nameof(fileManager));

        // Register default loaders
        RegisterDefaultLoaders();
    }

    /// <summary>
    /// Registers a resource loader for a specific type.
    /// </summary>
    public void RegisterLoader<T>(IResourceLoader<T> loader) where T : class
    {
        ThrowIfDisposed();
        _loaders[typeof(T)] = loader;
        _logger.LogDebug("Registered resource loader for type: {Type}", typeof(T).Name);
    }

    /// <summary>
    /// Loads or retrieves a cached resource.
    /// </summary>
    public async Task<T?> LoadResourceAsync<T>(string filename) where T : class
    {
        ThrowIfDisposed();

        if (string.IsNullOrWhiteSpace(filename))
            throw new ArgumentException("Filename cannot be null or empty", nameof(filename));

        // Check cache first
        if (_cache.TryGetValue(filename, out var cached))
        {
            if (cached.Resource is T resource)
            {
                cached.LastAccessTime = DateTime.UtcNow;
                _logger.LogDebug("Resource cache hit: {File}", filename);
                return resource;
            }
        }

        // Load the resource
        _logger.LogDebug("Loading resource: {File}", filename);

        if (!_loaders.TryGetValue(typeof(T), out var loader))
        {
            _logger.LogError("No loader registered for type: {Type}", typeof(T).Name);
            return null;
        }

        try
        {
            var typedLoader = (IResourceLoader<T>)loader;
            var loadedResource = await typedLoader.LoadAsync(_fileManager, filename);

            if (loadedResource != null)
            {
                AddResource(filename, loadedResource);
            }

            return loadedResource;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load resource: {File}", filename);
            return null;
        }
    }

    /// <summary>
    /// Adds a resource to the cache.
    /// </summary>
    public void AddResource<T>(string key, T resource) where T : class
    {
        ThrowIfDisposed();

        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be null or empty", nameof(key));

        if (resource == null)
            throw new ArgumentNullException(nameof(resource));

        var cached = new CachedResource
        {
            Resource = resource,
            Key = key,
            Type = typeof(T),
            LoadTime = DateTime.UtcNow,
            LastAccessTime = DateTime.UtcNow,
            EstimatedSize = EstimateSize(resource)
        };

        _cache[key] = cached;
        _logger.LogDebug("Cached resource: {Key} (type: {Type}, size: {Size} bytes)",
            key, typeof(T).Name, cached.EstimatedSize);
    }

    /// <summary>
    /// Retrieves a cached resource without loading.
    /// </summary>
    public T? GetResource<T>(string key) where T : class
    {
        ThrowIfDisposed();

        if (_cache.TryGetValue(key, out var cached))
        {
            if (cached.Resource is T resource)
            {
                cached.LastAccessTime = DateTime.UtcNow;
                return resource;
            }
        }

        return null;
    }

    /// <summary>
    /// Removes a resource from the cache and disposes it if applicable.
    /// </summary>
    public void UnloadResource(string key)
    {
        ThrowIfDisposed();

        if (_cache.TryRemove(key, out var cached))
        {
            DisposeResource(cached.Resource);
            _logger.LogDebug("Unloaded resource: {Key}", key);
        }
    }

    /// <summary>
    /// Clears all cached resources.
    /// </summary>
    public void ClearCache(bool disposeResources = true)
    {
        ThrowIfDisposed();

        var count = _cache.Count;

        if (disposeResources)
        {
            foreach (var cached in _cache.Values)
            {
                DisposeResource(cached.Resource);
            }
        }

        _cache.Clear();
        _logger.LogInformation("Cleared resource cache ({Count} resources)", count);
    }

    /// <summary>
    /// Preloads a set of resources asynchronously.
    /// </summary>
    public async Task PreloadResourcesAsync(IEnumerable<string> filenames)
    {
        ThrowIfDisposed();

        var tasks = new List<Task>();

        foreach (var filename in filenames)
        {
            // This is a simplified implementation - in reality you'd need to know the type
            // For now, we'll skip this as it requires more context
            _logger.LogWarning("PreloadResourcesAsync not fully implemented - requires type information");
        }

        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Registers default resource loaders.
    /// </summary>
    private void RegisterDefaultLoaders()
    {
        // Default loaders will be registered here
        // Full implementation in later phases when specific resource types are implemented
        _logger.LogDebug("Resource manager initialized with default loaders");
    }

    /// <summary>
    /// Estimates the memory size of a resource.
    /// </summary>
    private long EstimateSize(object resource)
    {
        // This is a simplified estimation
        // Real implementation would vary based on resource type
        return resource switch
        {
            byte[] bytes => bytes.Length,
            string str => str.Length * 2, // Rough estimate for UTF-16
            _ => 1024 // Default estimate
        };
    }

    /// <summary>
    /// Disposes a resource if it implements IDisposable.
    /// </summary>
    private void DisposeResource(object resource)
    {
        if (resource is IDisposable disposable)
        {
            try
            {
                disposable.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error disposing resource");
            }
        }
    }

    /// <summary>
    /// Throws if disposed.
    /// </summary>
    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(WmeResourceManager));
    }

    /// <summary>
    /// Disposes the resource manager and all cached resources.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        ClearCache(disposeResources: true);

        _disposed = true;
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Represents a cached resource with metadata.
    /// </summary>
    private class CachedResource
    {
        public object Resource { get; set; } = null!;
        public string Key { get; set; } = string.Empty;
        public Type Type { get; set; } = null!;
        public DateTime LoadTime { get; set; }
        public DateTime LastAccessTime { get; set; }
        public long EstimatedSize { get; set; }
    }
}

/// <summary>
/// Interface for resource loaders.
/// </summary>
public interface IResourceLoader
{
}

/// <summary>
/// Generic interface for resource loaders.
/// </summary>
public interface IResourceLoader<T> : IResourceLoader where T : class
{
    /// <summary>
    /// Loads a resource asynchronously.
    /// </summary>
    Task<T?> LoadAsync(IWmeFileManager fileManager, string filename);
}
