namespace WME.Graphics.Sprites;

/// <summary>
/// Represents a sub-frame within a sprite frame.
/// Sub-frames allow compositing multiple surfaces into a single frame.
/// </summary>
public class WmeSubFrame : IDisposable
{
    private bool _disposed;

    /// <summary>
    /// Gets or sets the surface to render.
    /// </summary>
    public IWmeSurface? Surface { get; set; }

    /// <summary>
    /// Gets or sets the source rectangle (null = entire surface).
    /// </summary>
    public Rectangle? SourceRect { get; set; }

    /// <summary>
    /// Gets or sets the X offset from the frame's hotspot.
    /// </summary>
    public int OffsetX { get; set; }

    /// <summary>
    /// Gets or sets the Y offset from the frame's hotspot.
    /// </summary>
    public int OffsetY { get; set; }

    /// <summary>
    /// Gets or sets the alpha transparency (0.0 = fully transparent, 1.0 = fully opaque).
    /// </summary>
    public float Alpha { get; set; } = 1.0f;

    /// <summary>
    /// Gets or sets whether to mirror horizontally.
    /// </summary>
    public bool MirrorX { get; set; }

    /// <summary>
    /// Gets or sets whether to mirror vertically.
    /// </summary>
    public bool MirrorY { get; set; }

    /// <summary>
    /// Gets or sets the decorative flag (for editor use).
    /// </summary>
    public bool IsDecorative { get; set; }

    /// <summary>
    /// Gets or sets the surface filename.
    /// </summary>
    public string? SurfaceFilename { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="WmeSubFrame"/> class.
    /// </summary>
    public WmeSubFrame()
    {
    }

    /// <summary>
    /// Initializes a new instance with a surface.
    /// </summary>
    public WmeSubFrame(IWmeSurface surface, int offsetX = 0, int offsetY = 0)
    {
        Surface = surface ?? throw new ArgumentNullException(nameof(surface));
        OffsetX = offsetX;
        OffsetY = offsetY;
    }

    /// <summary>
    /// Disposes the sub-frame.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        // Note: We don't dispose the surface here because it may be shared
        // Surface disposal is managed by the resource manager

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
