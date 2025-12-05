namespace WME.Core.UI;

/// <summary>
/// Represents a UI window in the game.
/// </summary>
/// <remarks>
/// Full implementation will be added in Phase 6: UI System.
/// </remarks>
public class WmeWindow : Objects.WmeObject
{
    /// <summary>
    /// Gets or sets the window position.
    /// </summary>
    public Point Position { get; set; }

    /// <summary>
    /// Gets or sets the window size.
    /// </summary>
    public Size Size { get; set; }
}

/// <summary>
/// Represents a 2D point.
/// </summary>
public record struct Point(int X, int Y);

/// <summary>
/// Represents a 2D size.
/// </summary>
public record struct Size(int Width, int Height);
