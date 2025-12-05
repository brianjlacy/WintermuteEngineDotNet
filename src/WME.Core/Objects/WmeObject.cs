namespace WME.Core.Objects;

/// <summary>
/// Base class for all game objects in Wintermute Engine.
/// Provides common functionality for update, render, and event handling.
/// </summary>
/// <remarks>
/// Full implementation will be added in the Base Classes phase.
/// </remarks>
public abstract class WmeObject : IDisposable
{
    /// <summary>
    /// Gets or sets the object name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets whether the object is visible.
    /// </summary>
    public bool Visible { get; set; } = true;

    /// <summary>
    /// Updates the object logic.
    /// </summary>
    /// <param name="deltaTime">Time elapsed since last update.</param>
    public virtual void Update(TimeSpan deltaTime) { }

    /// <summary>
    /// Renders the object.
    /// </summary>
    public virtual void Render() { }

    /// <summary>
    /// Releases resources used by this object.
    /// </summary>
    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
