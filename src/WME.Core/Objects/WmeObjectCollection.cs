namespace WME.Core.Objects;

/// <summary>
/// Generic collection for managing WME objects with automatic disposal and persistence support.
/// </summary>
/// <typeparam name="T">The type of objects in the collection (must derive from WmeBase).</typeparam>
public class WmeObjectCollection<T> : IDisposable, IEnumerable<T> where T : WmeBase
{
    private readonly List<T> _items = new();
    private readonly ILogger _logger;
    private bool _disposed;

    /// <summary>
    /// Gets the number of objects in the collection.
    /// </summary>
    public int Count => _items.Count;

    /// <summary>
    /// Gets or sets the object at the specified index.
    /// </summary>
    public T this[int index]
    {
        get
        {
            ThrowIfDisposed();
            return _items[index];
        }
        set
        {
            ThrowIfDisposed();
            _items[index] = value ?? throw new ArgumentNullException(nameof(value));
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WmeObjectCollection{T}"/> class.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    public WmeObjectCollection(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Adds an object to the collection.
    /// </summary>
    /// <param name="item">The object to add.</param>
    public void Add(T item)
    {
        ThrowIfDisposed();
        if (item == null) throw new ArgumentNullException(nameof(item));

        _items.Add(item);
    }

    /// <summary>
    /// Removes an object from the collection.
    /// </summary>
    /// <param name="item">The object to remove.</param>
    /// <returns>True if the object was removed, false if not found.</returns>
    public bool Remove(T item)
    {
        ThrowIfDisposed();
        return _items.Remove(item);
    }

    /// <summary>
    /// Removes the object at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the object to remove.</param>
    public void RemoveAt(int index)
    {
        ThrowIfDisposed();
        _items.RemoveAt(index);
    }

    /// <summary>
    /// Removes all objects from the collection.
    /// </summary>
    /// <param name="disposeItems">If true, disposes all removed items.</param>
    public void Clear(bool disposeItems = false)
    {
        ThrowIfDisposed();

        if (disposeItems)
        {
            foreach (var item in _items)
            {
                item?.Dispose();
            }
        }

        _items.Clear();
    }

    /// <summary>
    /// Determines whether the collection contains a specific object.
    /// </summary>
    /// <param name="item">The object to locate.</param>
    /// <returns>True if the object is found; otherwise, false.</returns>
    public bool Contains(T item)
    {
        ThrowIfDisposed();
        return _items.Contains(item);
    }

    /// <summary>
    /// Searches for an object by name.
    /// </summary>
    /// <param name="name">The name to search for.</param>
    /// <returns>The first object with the specified name, or null if not found.</returns>
    public T? FindByName(string name)
    {
        ThrowIfDisposed();
        return _items.FirstOrDefault(item =>
            string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Searches for an object by ID.
    /// </summary>
    /// <param name="id">The ID to search for.</param>
    /// <returns>The object with the specified ID, or null if not found.</returns>
    public T? FindById(long id)
    {
        ThrowIfDisposed();
        return _items.FirstOrDefault(item => item.Id == id);
    }

    /// <summary>
    /// Persists the collection state.
    /// </summary>
    /// <param name="persistMgr">The persistence manager.</param>
    /// <returns>True if persistence succeeded.</returns>
    public bool Persist(IPersistenceManager persistMgr)
    {
        ThrowIfDisposed();

        var count = _items.Count;
        persistMgr.Transfer("Count", ref count);

        if (persistMgr.IsSaving)
        {
            foreach (var item in _items)
            {
                persistMgr.TransferObject(item.Name ?? "Item", item);
            }
        }
        else
        {
            _items.Clear();
            for (int i = 0; i < count; i++)
            {
                // Note: Loading logic would need to create instances based on type information
                // This is a simplified version - full implementation in Phase 5
                _logger.LogWarning("WmeObjectCollection loading not fully implemented yet");
            }
        }

        return true;
    }

    /// <summary>
    /// Disposes the collection and optionally all contained objects.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the collection.
    /// </summary>
    /// <param name="disposing">True to release managed resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            // Dispose all items
            foreach (var item in _items)
            {
                item?.Dispose();
            }
            _items.Clear();
        }

        _disposed = true;
    }

    /// <summary>
    /// Throws if the collection has been disposed.
    /// </summary>
    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(GetType().Name);
        }
    }

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    public IEnumerator<T> GetEnumerator()
    {
        ThrowIfDisposed();
        return _items.GetEnumerator();
    }

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
