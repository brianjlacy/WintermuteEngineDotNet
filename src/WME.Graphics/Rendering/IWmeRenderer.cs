namespace WME.Graphics.Rendering;

/// <summary>
/// Core rendering interface for Wintermute Engine.
/// Provides 2D and 3D rendering capabilities with multiple backends (OpenGL, Vulkan).
/// </summary>
public interface IWmeRenderer : IDisposable
{
    /// <summary>
    /// Gets the current render target width in pixels.
    /// </summary>
    int Width { get; }

    /// <summary>
    /// Gets the current render target height in pixels.
    /// </summary>
    int Height { get; }

    /// <summary>
    /// Gets whether the renderer is in windowed mode.
    /// </summary>
    bool Windowed { get; }

    /// <summary>
    /// Initializes the rendering system.
    /// </summary>
    /// <returns>Task representing the async initialization operation.</returns>
    Task InitializeAsync();

    /// <summary>
    /// Begins a new frame. Must be called before any draw operations.
    /// </summary>
    void BeginFrame();

    /// <summary>
    /// Ends the current frame and presents it to the display.
    /// </summary>
    void EndFrame();

    // === 2D Rendering Operations ===

    /// <summary>
    /// Draws a surface at the specified position.
    /// </summary>
    /// <param name="surface">The surface to draw.</param>
    /// <param name="x">X coordinate.</param>
    /// <param name="y">Y coordinate.</param>
    /// <param name="sourceRect">Optional source rectangle (null = entire surface).</param>
    /// <param name="blendMode">Blend mode to use.</param>
    /// <param name="alpha">Alpha transparency (0.0 = fully transparent, 1.0 = fully opaque).</param>
    /// <param name="mirrorX">Mirror horizontally if true.</param>
    /// <param name="mirrorY">Mirror vertically if true.</param>
    void DrawSurface(
        IWmeSurface surface,
        int x, int y,
        Rectangle? sourceRect = null,
        BlendMode blendMode = BlendMode.Normal,
        float alpha = 1.0f,
        bool mirrorX = false,
        bool mirrorY = false);

    /// <summary>
    /// Draws a surface with transformation (rotation, scaling).
    /// </summary>
    /// <param name="surface">The surface to draw.</param>
    /// <param name="x">X coordinate.</param>
    /// <param name="y">Y coordinate.</param>
    /// <param name="hotX">Hotspot X offset for rotation/scaling.</param>
    /// <param name="hotY">Hotspot Y offset for rotation/scaling.</param>
    /// <param name="sourceRect">Optional source rectangle.</param>
    /// <param name="scaleX">Horizontal scale factor.</param>
    /// <param name="scaleY">Vertical scale factor.</param>
    /// <param name="rotation">Rotation angle in degrees.</param>
    /// <param name="alpha">Alpha transparency.</param>
    /// <param name="blendMode">Blend mode to use.</param>
    /// <param name="mirrorX">Mirror horizontally if true.</param>
    /// <param name="mirrorY">Mirror vertically if true.</param>
    void DrawSurfaceTransform(
        IWmeSurface surface,
        int x, int y,
        int hotX, int hotY,
        Rectangle? sourceRect,
        float scaleX, float scaleY,
        float rotation,
        float alpha,
        BlendMode blendMode,
        bool mirrorX, bool mirrorY);

    /// <summary>
    /// Draws a line between two points.
    /// </summary>
    /// <param name="x1">Start X coordinate.</param>
    /// <param name="y1">Start Y coordinate.</param>
    /// <param name="x2">End X coordinate.</param>
    /// <param name="y2">End Y coordinate.</param>
    /// <param name="color">Line color.</param>
    void DrawLine(int x1, int y1, int x2, int y2, Color color);

    /// <summary>
    /// Draws a rectangle outline.
    /// </summary>
    /// <param name="rect">The rectangle to draw.</param>
    /// <param name="color">Line color.</param>
    /// <param name="width">Line width in pixels.</param>
    void DrawRect(Rectangle rect, Color color, int width = 1);

    /// <summary>
    /// Fills a rectangle with a solid color.
    /// </summary>
    /// <param name="rect">The rectangle to fill.</param>
    /// <param name="color">Fill color.</param>
    void FillRect(Rectangle rect, Color color);

    /// <summary>
    /// Fades the entire screen (or a region) to a specific color.
    /// </summary>
    /// <param name="color">Color to fade to.</param>
    /// <param name="rect">Optional region to fade (null = entire screen).</param>
    void FadeToColor(Color color, Rectangle? rect = null);

    // === 3D Rendering Operations ===

    /// <summary>
    /// Sets up the renderer for 3D rendering with the specified camera.
    /// </summary>
    /// <param name="camera">The camera to use for 3D rendering.</param>
    void Setup3D(IWmeCamera3D camera);

    /// <summary>
    /// Returns the renderer to 2D mode.
    /// </summary>
    void Setup2D();

    /// <summary>
    /// Draws a 3D model with the specified transformation.
    /// </summary>
    /// <param name="model">The 3D model to draw.</param>
    /// <param name="transform">Transformation matrix.</param>
    void DrawModel(IWmeModel3D model, Matrix4x4 transform);

    // === Surface Management ===

    /// <summary>
    /// Creates a new empty surface with the specified dimensions.
    /// </summary>
    /// <param name="width">Surface width in pixels.</param>
    /// <param name="height">Surface height in pixels.</param>
    /// <returns>The created surface.</returns>
    IWmeSurface CreateSurface(int width, int height);

    /// <summary>
    /// Loads a surface from a file.
    /// </summary>
    /// <param name="filename">Path to the image file.</param>
    /// <param name="colorKey">Optional color key for transparency.</param>
    /// <returns>The loaded surface.</returns>
    IWmeSurface LoadSurface(string filename, Color? colorKey = null);

    // === Utility Operations ===

    /// <summary>
    /// Captures the current frame as an image.
    /// </summary>
    /// <returns>The screenshot image.</returns>
    IWmeImage TakeScreenshot();

    /// <summary>
    /// Performs hit testing to find the object at the specified screen coordinates.
    /// </summary>
    /// <param name="x">Screen X coordinate.</param>
    /// <param name="y">Screen Y coordinate.</param>
    /// <returns>The object at the specified position, or null if none.</returns>
    object? GetObjectAt(int x, int y);
}

/// <summary>
/// Blend modes for rendering operations.
/// </summary>
public enum BlendMode
{
    /// <summary>
    /// Normal alpha blending.
    /// </summary>
    Normal,

    /// <summary>
    /// Additive blending (colors are added).
    /// </summary>
    Additive,

    /// <summary>
    /// Subtractive blending (colors are subtracted).
    /// </summary>
    Subtractive,

    /// <summary>
    /// Multiplicative blending (colors are multiplied).
    /// </summary>
    Multiply
}

/// <summary>
/// Represents a rectangle for rendering operations.
/// </summary>
public record struct Rectangle(int X, int Y, int Width, int Height)
{
    /// <summary>
    /// Gets the right edge of the rectangle.
    /// </summary>
    public readonly int Right => X + Width;

    /// <summary>
    /// Gets the bottom edge of the rectangle.
    /// </summary>
    public readonly int Bottom => Y + Height;
}

/// <summary>
/// Represents an RGBA color.
/// </summary>
public record struct Color(byte R, byte G, byte B, byte A = 255)
{
    /// <summary>
    /// Creates a color from float values (0.0-1.0).
    /// </summary>
    public static Color FromFloat(float r, float g, float b, float a = 1.0f) =>
        new((byte)(r * 255), (byte)(g * 255), (byte)(b * 255), (byte)(a * 255));

    /// <summary>
    /// Common colors.
    /// </summary>
    public static readonly Color White = new(255, 255, 255);
    public static readonly Color Black = new(0, 0, 0);
    public static readonly Color Red = new(255, 0, 0);
    public static readonly Color Green = new(0, 255, 0);
    public static readonly Color Blue = new(0, 0, 255);
    public static readonly Color Transparent = new(0, 0, 0, 0);
}

/// <summary>
/// 4x4 transformation matrix for 3D operations.
/// </summary>
public record struct Matrix4x4(
    float M11, float M12, float M13, float M14,
    float M21, float M22, float M23, float M24,
    float M31, float M32, float M33, float M34,
    float M41, float M42, float M43, float M44)
{
    /// <summary>
    /// Identity matrix.
    /// </summary>
    public static readonly Matrix4x4 Identity = new(
        1, 0, 0, 0,
        0, 1, 0, 0,
        0, 0, 1, 0,
        0, 0, 0, 1);
}
