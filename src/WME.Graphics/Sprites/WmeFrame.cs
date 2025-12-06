namespace WME.Graphics.Sprites;

/// <summary>
/// Represents a single frame in a sprite animation.
/// </summary>
public class WmeFrame : IDisposable
{
    private readonly List<WmeSubFrame> _subframes = new();
    private bool _disposed;

    /// <summary>
    /// Gets or sets the frame delay in milliseconds.
    /// </summary>
    public int Delay { get; set; } = 100;

    /// <summary>
    /// Gets or sets the hotspot X coordinate.
    /// </summary>
    public int HotspotX { get; set; }

    /// <summary>
    /// Gets or sets the hotspot Y coordinate.
    /// </summary>
    public int HotspotY { get; set; }

    /// <summary>
    /// Gets the list of sub-frames.
    /// </summary>
    public IReadOnlyList<WmeSubFrame> SubFrames => _subframes;

    /// <summary>
    /// Gets or sets the associated sound filename.
    /// </summary>
    public string? SoundFilename { get; set; }

    /// <summary>
    /// Gets or sets whether to start playing sound at the beginning of this frame.
    /// </summary>
    public bool PlaySoundAtStart { get; set; }

    /// <summary>
    /// Gets or sets the frame name (for keyframe support).
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets whether this is a keyframe.
    /// </summary>
    public bool IsKeyframe { get; set; }

    /// <summary>
    /// Gets or sets whether this frame should be killed (removed during animation).
    /// </summary>
    public bool ShouldKill { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="WmeFrame"/> class.
    /// </summary>
    public WmeFrame()
    {
    }

    /// <summary>
    /// Adds a sub-frame to this frame.
    /// </summary>
    public void AddSubFrame(WmeSubFrame subframe)
    {
        ThrowIfDisposed();

        if (subframe == null)
            throw new ArgumentNullException(nameof(subframe));

        _subframes.Add(subframe);
    }

    /// <summary>
    /// Removes a sub-frame from this frame.
    /// </summary>
    public bool RemoveSubFrame(WmeSubFrame subframe)
    {
        ThrowIfDisposed();
        return _subframes.Remove(subframe);
    }

    /// <summary>
    /// Clears all sub-frames.
    /// </summary>
    public void ClearSubFrames()
    {
        ThrowIfDisposed();

        foreach (var subframe in _subframes)
        {
            subframe.Dispose();
        }

        _subframes.Clear();
    }

    /// <summary>
    /// Gets the total width of this frame.
    /// </summary>
    public int GetWidth()
    {
        ThrowIfDisposed();

        if (_subframes.Count == 0)
            return 0;

        int maxRight = 0;
        foreach (var subframe in _subframes)
        {
            if (subframe.Surface != null)
            {
                int right = subframe.OffsetX + subframe.Surface.Width;
                if (right > maxRight)
                    maxRight = right;
            }
        }

        return maxRight;
    }

    /// <summary>
    /// Gets the total height of this frame.
    /// </summary>
    public int GetHeight()
    {
        ThrowIfDisposed();

        if (_subframes.Count == 0)
            return 0;

        int maxBottom = 0;
        foreach (var subframe in _subframes)
        {
            if (subframe.Surface != null)
            {
                int bottom = subframe.OffsetY + subframe.Surface.Height;
                if (bottom > maxBottom)
                    maxBottom = bottom;
            }
        }

        return maxBottom;
    }

    /// <summary>
    /// Throws if disposed.
    /// </summary>
    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(WmeFrame));
    }

    /// <summary>
    /// Disposes the frame.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        ClearSubFrames();

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
