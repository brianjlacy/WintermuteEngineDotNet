namespace WME.Graphics.Sprites;

using WME.Core.Objects;

/// <summary>
/// Represents an animated sprite composed of multiple frames.
/// </summary>
public class WmeSprite : WmeObject
{
    private readonly List<WmeFrame> _frames = new();
    private int _currentFrame;
    private TimeSpan _frameTime;
    private bool _paused;
    private bool _finished;
    private int _loopStart;

    /// <summary>
    /// Gets the list of frames.
    /// </summary>
    public IReadOnlyList<WmeFrame> Frames => _frames;

    /// <summary>
    /// Gets or sets the current frame index.
    /// </summary>
    public int CurrentFrame
    {
        get => _currentFrame;
        set
        {
            ThrowIfDisposed();

            if (value < 0 || value >= _frames.Count)
                value = 0;

            if (_currentFrame != value)
            {
                _currentFrame = value;
                _frameTime = TimeSpan.Zero;
                OnPropertyChanged(nameof(CurrentFrame));
            }
        }
    }

    /// <summary>
    /// Gets whether the sprite animation is paused.
    /// </summary>
    public bool IsPaused
    {
        get => _paused;
        set
        {
            if (_paused != value)
            {
                _paused = value;
                OnPropertyChanged(nameof(IsPaused));
            }
        }
    }

    /// <summary>
    /// Gets whether the sprite animation has finished.
    /// </summary>
    public bool IsFinished => _finished;

    /// <summary>
    /// Gets or sets whether the sprite should loop.
    /// </summary>
    public bool Looping { get; set; } = true;

    /// <summary>
    /// Gets or sets the loop start frame (for partial loops).
    /// </summary>
    public int LoopStart
    {
        get => _loopStart;
        set
        {
            if (value < 0 || value >= _frames.Count)
                value = 0;

            _loopStart = value;
        }
    }

    /// <summary>
    /// Gets or sets the sprite filename.
    /// </summary>
    public string? Filename { get; set; }

    /// <summary>
    /// Gets whether the sprite has any frames.
    /// </summary>
    public bool HasFrames => _frames.Count > 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="WmeSprite"/> class.
    /// </summary>
    public WmeSprite(ILogger logger) : base(logger)
    {
    }

    /// <summary>
    /// Adds a frame to the sprite.
    /// </summary>
    public void AddFrame(WmeFrame frame)
    {
        ThrowIfDisposed();

        if (frame == null)
            throw new ArgumentNullException(nameof(frame));

        _frames.Add(frame);
    }

    /// <summary>
    /// Removes a frame from the sprite.
    /// </summary>
    public bool RemoveFrame(WmeFrame frame)
    {
        ThrowIfDisposed();
        return _frames.Remove(frame);
    }

    /// <summary>
    /// Removes a frame at the specified index.
    /// </summary>
    public void RemoveFrameAt(int index)
    {
        ThrowIfDisposed();

        if (index < 0 || index >= _frames.Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        var frame = _frames[index];
        _frames.RemoveAt(index);
        frame.Dispose();

        // Adjust current frame if needed
        if (_currentFrame >= _frames.Count)
            _currentFrame = Math.Max(0, _frames.Count - 1);
    }

    /// <summary>
    /// Clears all frames.
    /// </summary>
    public void ClearFrames()
    {
        ThrowIfDisposed();

        foreach (var frame in _frames)
        {
            frame.Dispose();
        }

        _frames.Clear();
        _currentFrame = 0;
        _frameTime = TimeSpan.Zero;
        _finished = false;
    }

    /// <summary>
    /// Gets a frame by index.
    /// </summary>
    public WmeFrame? GetFrame(int index)
    {
        ThrowIfDisposed();

        if (index < 0 || index >= _frames.Count)
            return null;

        return _frames[index];
    }

    /// <summary>
    /// Gets a frame by name (keyframe).
    /// </summary>
    public WmeFrame? GetFrameByName(string name)
    {
        ThrowIfDisposed();

        if (string.IsNullOrWhiteSpace(name))
            return null;

        return _frames.FirstOrDefault(f =>
            f.Name?.Equals(name, StringComparison.OrdinalIgnoreCase) == true);
    }

    /// <summary>
    /// Gets the current frame.
    /// </summary>
    public WmeFrame? GetCurrentFrame()
    {
        ThrowIfDisposed();

        if (_currentFrame < 0 || _currentFrame >= _frames.Count)
            return null;

        return _frames[_currentFrame];
    }

    /// <summary>
    /// Resets the sprite animation to the first frame.
    /// </summary>
    public void Reset()
    {
        ThrowIfDisposed();

        _currentFrame = 0;
        _frameTime = TimeSpan.Zero;
        _finished = false;
        _paused = false;
    }

    /// <summary>
    /// Updates the sprite animation.
    /// </summary>
    public override bool Update(TimeSpan deltaTime)
    {
        if (!base.Update(deltaTime))
            return false;

        if (_paused || _finished || _frames.Count == 0)
            return true;

        _frameTime += deltaTime;

        var currentFrameObj = GetCurrentFrame();
        if (currentFrameObj == null)
            return true;

        // Check if it's time to advance to the next frame
        while (_frameTime.TotalMilliseconds >= currentFrameObj.Delay)
        {
            _frameTime -= TimeSpan.FromMilliseconds(currentFrameObj.Delay);

            // Advance to next frame
            _currentFrame++;

            // Check for loop
            if (_currentFrame >= _frames.Count)
            {
                if (Looping)
                {
                    _currentFrame = _loopStart;
                }
                else
                {
                    _currentFrame = _frames.Count - 1;
                    _finished = true;
                    break;
                }
            }

            currentFrameObj = GetCurrentFrame();
            if (currentFrameObj == null)
                break;

            // Handle kill frame
            if (currentFrameObj.ShouldKill)
            {
                _finished = true;
                break;
            }
        }

        return true;
    }

    /// <summary>
    /// Renders the current frame.
    /// </summary>
    public override bool Render()
    {
        if (!base.Render())
            return false;

        // Rendering is handled by the sprite renderer
        // This is a placeholder for custom rendering logic

        return true;
    }

    /// <summary>
    /// Gets the width of the current frame.
    /// </summary>
    public int GetWidth()
    {
        var frame = GetCurrentFrame();
        return frame?.GetWidth() ?? 0;
    }

    /// <summary>
    /// Gets the height of the current frame.
    /// </summary>
    public int GetHeight()
    {
        var frame = GetCurrentFrame();
        return frame?.GetHeight() ?? 0;
    }

    /// <summary>
    /// Disposes the sprite.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            ClearFrames();
        }

        base.Dispose(disposing);
    }
}
