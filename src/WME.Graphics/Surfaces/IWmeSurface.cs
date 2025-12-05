namespace WME.Graphics.Surfaces;

/// <summary>
/// Represents a 2D rendering surface (texture).
/// </summary>
public interface IWmeSurface : IDisposable
{
    /// <summary>
    /// Gets the surface width in pixels.
    /// </summary>
    int Width { get; }

    /// <summary>
    /// Gets the surface height in pixels.
    /// </summary>
    int Height { get; }

    /// <summary>
    /// Gets whether this surface has an alpha channel.
    /// </summary>
    bool HasAlpha { get; }

    /// <summary>
    /// Gets the filename this surface was loaded from (if applicable).
    /// </summary>
    string? Filename { get; }
}
