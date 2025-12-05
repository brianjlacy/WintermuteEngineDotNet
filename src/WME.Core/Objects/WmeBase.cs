namespace WME.Core.Objects;

/// <summary>
/// Base class for most Wintermute Engine objects.
/// Provides core functionality for object identification, lifecycle management, and persistence.
/// </summary>
public abstract class WmeBase : IDisposable
{
    private static long _nextId = 1;
    private bool _disposed;

    /// <summary>
    /// Gets the unique ID for this object.
    /// </summary>
    public long Id { get; }

    /// <summary>
    /// Gets or sets the object name (optional, for debugging and scripting).
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets the logger instance for this object.
    /// </summary>
    protected ILogger Logger { get; }

    /// <summary>
    /// Gets whether this object has been disposed.
    /// </summary>
    public bool IsDisposed => _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="WmeBase"/> class.
    /// </summary>
    /// <param name="logger">Logger instance for this object.</param>
    protected WmeBase(ILogger logger)
    {
        Id = Interlocked.Increment(ref _nextId);
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Persists the object state to/from a persistence manager.
    /// </summary>
    /// <param name="persistMgr">The persistence manager.</param>
    /// <returns>True if persistence succeeded.</returns>
    public virtual bool Persist(IPersistenceManager persistMgr)
    {
        if (persistMgr == null) throw new ArgumentNullException(nameof(persistMgr));

        persistMgr.Transfer(nameof(Name), ref Name);
        return true;
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the object and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            // Dispose managed resources
            DisposeManagedResources();
        }

        // Dispose unmanaged resources
        DisposeUnmanagedResources();

        _disposed = true;
    }

    /// <summary>
    /// Disposes managed resources. Override in derived classes to dispose managed resources.
    /// </summary>
    protected virtual void DisposeManagedResources()
    {
    }

    /// <summary>
    /// Disposes unmanaged resources. Override in derived classes to dispose unmanaged resources.
    /// </summary>
    protected virtual void DisposeUnmanagedResources()
    {
    }

    /// <summary>
    /// Throws an <see cref="ObjectDisposedException"/> if this object has been disposed.
    /// </summary>
    protected void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(GetType().Name);
        }
    }

    /// <summary>
    /// Returns a string representation of this object.
    /// </summary>
    public override string ToString()
    {
        return string.IsNullOrEmpty(Name)
            ? $"{GetType().Name}#{Id}"
            : $"{GetType().Name}#{Id} ({Name})";
    }
}

/// <summary>
/// Interface for persistence management (save/load game state).
/// </summary>
/// <remarks>
/// Full implementation will be added in Phase 5: Adventure Module.
/// </remarks>
public interface IPersistenceManager
{
    /// <summary>
    /// Gets whether the persistence manager is currently saving (true) or loading (false).
    /// </summary>
    bool IsSaving { get; }

    /// <summary>
    /// Transfers a value field (reads during load, writes during save).
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="name">Field name for identification.</param>
    /// <param name="value">Reference to the value to transfer.</param>
    void Transfer<T>(string name, ref T? value);

    /// <summary>
    /// Transfers an object that supports persistence.
    /// </summary>
    /// <param name="name">Field name for identification.</param>
    /// <param name="obj">The object to transfer.</param>
    void TransferObject(string name, WmeBase? obj);
}
