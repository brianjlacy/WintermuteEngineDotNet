namespace WME.Core.Resources;

/// <summary>
/// Resource management interface for caching and lifecycle management of game assets.
/// </summary>
public interface IWmeResourceManager : IDisposable
{
    /// <summary>
    /// Loads or retrieves a cached resource.
    /// </summary>
    /// <typeparam name="T">The resource type.</typeparam>
    /// <param name="filename">Path to the resource file.</param>
    /// <returns>The loaded resource, or null if loading failed.</returns>
    Task<T?> LoadResourceAsync<T>(string filename) where T : class;

    /// <summary>
    /// Adds a resource to the cache.
    /// </summary>
    /// <typeparam name="T">The resource type.</typeparam>
    /// <param name="key">Unique key for the resource.</param>
    /// <param name="resource">The resource instance.</param>
    void AddResource<T>(string key, T resource) where T : class;

    /// <summary>
    /// Retrieves a cached resource without loading.
    /// </summary>
    /// <typeparam name="T">The resource type.</typeparam>
    /// <param name="key">The resource key.</param>
    /// <returns>The cached resource, or null if not found.</returns>
    T? GetResource<T>(string key) where T : class;

    /// <summary>
    /// Removes a resource from the cache and disposes it if applicable.
    /// </summary>
    /// <param name="key">The resource key.</param>
    void UnloadResource(string key);

    /// <summary>
    /// Clears all cached resources.
    /// </summary>
    /// <param name="disposeResources">If true, dispose resources that implement IDisposable.</param>
    void ClearCache(bool disposeResources = true);

    /// <summary>
    /// Gets the total number of cached resources.
    /// </summary>
    int CachedResourceCount { get; }

    /// <summary>
    /// Gets the approximate memory usage of cached resources in bytes.
    /// </summary>
    long CachedMemoryUsage { get; }

    /// <summary>
    /// Preloads a set of resources asynchronously.
    /// </summary>
    /// <param name="filenames">List of resource files to preload.</param>
    /// <returns>Task representing the preload operation.</returns>
    Task PreloadResourcesAsync(IEnumerable<string> filenames);
}
