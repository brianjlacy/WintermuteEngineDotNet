namespace WME.Graphics.Rendering;

using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Maths;
using SkiaSharp;
using WME.Core.Files;

/// <summary>
/// OpenGL-based renderer implementation using Silk.NET and SkiaSharp.
/// </summary>
public class OpenGLRenderer : IWmeRenderer
{
    private readonly ILogger<OpenGLRenderer> _logger;
    private readonly IWmeFileManager _fileManager;
    private IWindow? _window;
    private GL? _gl;
    private SKSurface? _skiaSurface;
    private GRContext? _grContext;
    private SKCanvas? _canvas;

    private readonly Dictionary<string, IWmeSurface> _surfaceCache = new(StringComparer.OrdinalIgnoreCase);
    private bool _disposed;
    private bool _is3DMode;

    /// <summary>
    /// Gets the current render target width in pixels.
    /// </summary>
    public int Width => _window?.Size.X ?? 0;

    /// <summary>
    /// Gets the current render target height in pixels.
    /// </summary>
    public int Height => _window?.Size.Y ?? 0;

    /// <summary>
    /// Gets whether the renderer is in windowed mode.
    /// </summary>
    public bool Windowed { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenGLRenderer"/> class.
    /// </summary>
    public OpenGLRenderer(ILogger<OpenGLRenderer> logger, IWmeFileManager fileManager)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _fileManager = fileManager ?? throw new ArgumentNullException(nameof(fileManager));
        Windowed = true;
    }

    /// <summary>
    /// Initializes the rendering system.
    /// </summary>
    public async Task InitializeAsync()
    {
        ThrowIfDisposed();

        await Task.Run(() =>
        {
            // Create window with Silk.NET
            var options = WindowOptions.Default;
            options.Size = new Vector2D<int>(1920, 1080);
            options.Title = "Wintermute Engine";
            options.VSync = true;
            options.ShouldSwapAutomatically = false; // We'll swap manually

            _window = Window.Create(options);
            _window.Load += OnWindowLoad;
            _window.Render += OnWindowRender;
            _window.Resize += OnWindowResize;
            _window.Closing += OnWindowClosing;

            _logger.LogInformation("OpenGL Renderer initialized: {Width}x{Height}, Windowed: {Windowed}",
                Width, Height, Windowed);
        });
    }

    /// <summary>
    /// Called when the window is loaded.
    /// </summary>
    private void OnWindowLoad()
    {
        // Initialize OpenGL context
        _gl = _window!.CreateOpenGL();

        _gl.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
        _gl.Enable(EnableCap.Blend);
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        // Initialize Skia for 2D rendering
        var grGlInterface = GRGlInterface.Create((name) =>
        {
            return _gl.Context.TryGetProcAddress(name, out var addr) ? addr : IntPtr.Zero;
        });

        _grContext = GRContext.CreateGl(grGlInterface);
        CreateSkiaSurface();

        _logger.LogInformation("OpenGL context created, Skia initialized");
    }

    /// <summary>
    /// Creates the Skia surface for 2D rendering.
    /// </summary>
    private void CreateSkiaSurface()
    {
        if (_grContext == null) return;

        _skiaSurface?.Dispose();

        var frameBufferInfo = new GRGlFramebufferInfo(
            fboId: 0,
            format: GRPixelConfig.Rgba8888.ToGlSizedFormat());

        var backendRenderTarget = new GRBackendRenderTarget(
            width: Width,
            height: Height,
            sampleCount: 0,
            stencilBits: 8,
            glInfo: frameBufferInfo);

        _skiaSurface = SKSurface.Create(
            context: _grContext,
            renderTarget: backendRenderTarget,
            origin: GRSurfaceOrigin.BottomLeft,
            colorType: SKColorType.Rgba8888);

        _canvas = _skiaSurface?.Canvas;

        _logger.LogDebug("Skia surface created: {Width}x{Height}", Width, Height);
    }

    /// <summary>
    /// Called when the window is resized.
    /// </summary>
    private void OnWindowResize(Vector2D<int> newSize)
    {
        if (_gl != null)
        {
            _gl.Viewport(0, 0, (uint)newSize.X, (uint)newSize.Y);
            CreateSkiaSurface();
        }
    }

    /// <summary>
    /// Called when the window is rendering.
    /// </summary>
    private void OnWindowRender(double deltaTime)
    {
        // This is called by the window's render loop
        // The actual rendering is done by the engine via BeginFrame/EndFrame
    }

    /// <summary>
    /// Called when the window is closing.
    /// </summary>
    private void OnWindowClosing()
    {
        _logger.LogInformation("Window closing");
    }

    /// <summary>
    /// Begins a new frame.
    /// </summary>
    public void BeginFrame()
    {
        ThrowIfDisposed();

        _gl?.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        if (_canvas != null)
        {
            _canvas.Clear(SKColors.Black);
            _canvas.Save();
        }
    }

    /// <summary>
    /// Ends the current frame and presents it.
    /// </summary>
    public void EndFrame()
    {
        ThrowIfDisposed();

        if (_canvas != null)
        {
            _canvas.Restore();
            _canvas.Flush();
        }

        _grContext?.Flush();
        _window?.SwapBuffers();
    }

    /// <summary>
    /// Draws a surface at the specified position.
    /// </summary>
    public void DrawSurface(
        IWmeSurface surface,
        int x, int y,
        Rectangle? sourceRect = null,
        BlendMode blendMode = BlendMode.Normal,
        float alpha = 1.0f,
        bool mirrorX = false,
        bool mirrorY = false)
    {
        ThrowIfDisposed();

        if (surface is not OpenGLSurface glSurface || _canvas == null)
            return;

        using var paint = CreatePaint(blendMode, alpha);

        var destRect = sourceRect.HasValue
            ? new SKRect(x, y, x + sourceRect.Value.Width, y + sourceRect.Value.Height)
            : new SKRect(x, y, x + surface.Width, y + surface.Height);

        SKRect srcRect = sourceRect.HasValue
            ? new SKRect(sourceRect.Value.X, sourceRect.Value.Y,
                sourceRect.Value.X + sourceRect.Value.Width,
                sourceRect.Value.Y + sourceRect.Value.Height)
            : new SKRect(0, 0, surface.Width, surface.Height);

        _canvas.Save();

        // Apply mirroring
        if (mirrorX || mirrorY)
        {
            var scaleX = mirrorX ? -1.0f : 1.0f;
            var scaleY = mirrorY ? -1.0f : 1.0f;
            var translateX = mirrorX ? destRect.Width : 0;
            var translateY = mirrorY ? destRect.Height : 0;

            _canvas.Translate(x + translateX, y + translateY);
            _canvas.Scale(scaleX, scaleY);
            _canvas.Translate(-x, -y);
        }

        _canvas.DrawImage(glSurface.Image, srcRect, destRect, paint);
        _canvas.Restore();
    }

    /// <summary>
    /// Draws a surface with transformation.
    /// </summary>
    public void DrawSurfaceTransform(
        IWmeSurface surface,
        int x, int y,
        int hotX, int hotY,
        Rectangle? sourceRect,
        float scaleX, float scaleY,
        float rotation,
        float alpha,
        BlendMode blendMode,
        bool mirrorX, bool mirrorY)
    {
        ThrowIfDisposed();

        if (surface is not OpenGLSurface glSurface || _canvas == null)
            return;

        using var paint = CreatePaint(blendMode, alpha);

        _canvas.Save();

        // Apply transformations in correct order
        _canvas.Translate(x, y);
        _canvas.RotateDegrees(rotation);
        _canvas.Scale(
            scaleX * (mirrorX ? -1 : 1),
            scaleY * (mirrorY ? -1 : 1));
        _canvas.Translate(-hotX, -hotY);

        SKRect srcRect = sourceRect.HasValue
            ? new SKRect(sourceRect.Value.X, sourceRect.Value.Y,
                sourceRect.Value.X + sourceRect.Value.Width,
                sourceRect.Value.Y + sourceRect.Value.Height)
            : new SKRect(0, 0, surface.Width, surface.Height);

        var destRect = new SKRect(0, 0, srcRect.Width, srcRect.Height);

        _canvas.DrawImage(glSurface.Image, srcRect, destRect, paint);
        _canvas.Restore();
    }

    /// <summary>
    /// Draws a line between two points.
    /// </summary>
    public void DrawLine(int x1, int y1, int x2, int y2, Color color)
    {
        ThrowIfDisposed();

        if (_canvas == null) return;

        using var paint = new SKPaint
        {
            Color = new SKColor(color.R, color.G, color.B, color.A),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1,
            IsAntialias = true
        };

        _canvas.DrawLine(x1, y1, x2, y2, paint);
    }

    /// <summary>
    /// Draws a rectangle outline.
    /// </summary>
    public void DrawRect(Rectangle rect, Color color, int width = 1)
    {
        ThrowIfDisposed();

        if (_canvas == null) return;

        using var paint = new SKPaint
        {
            Color = new SKColor(color.R, color.G, color.B, color.A),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = width,
            IsAntialias = true
        };

        _canvas.DrawRect(rect.X, rect.Y, rect.Width, rect.Height, paint);
    }

    /// <summary>
    /// Fills a rectangle with a solid color.
    /// </summary>
    public void FillRect(Rectangle rect, Color color)
    {
        ThrowIfDisposed();

        if (_canvas == null) return;

        using var paint = new SKPaint
        {
            Color = new SKColor(color.R, color.G, color.B, color.A),
            Style = SKPaintStyle.Fill
        };

        _canvas.DrawRect(rect.X, rect.Y, rect.Width, rect.Height, paint);
    }

    /// <summary>
    /// Fades the screen to a specific color.
    /// </summary>
    public void FadeToColor(Color color, Rectangle? rect = null)
    {
        ThrowIfDisposed();

        if (_canvas == null) return;

        using var paint = new SKPaint
        {
            Color = new SKColor(color.R, color.G, color.B, color.A),
            Style = SKPaintStyle.Fill
        };

        if (rect.HasValue)
        {
            _canvas.DrawRect(rect.Value.X, rect.Value.Y, rect.Value.Width, rect.Value.Height, paint);
        }
        else
        {
            _canvas.DrawRect(0, 0, Width, Height, paint);
        }
    }

    /// <summary>
    /// Sets up 3D rendering mode.
    /// </summary>
    public void Setup3D(IWmeCamera3D camera)
    {
        ThrowIfDisposed();

        // TODO: Implement 3D setup with camera matrices
        // This will be implemented when 3D rendering is added
        _is3DMode = true;
        _logger.LogDebug("Setup 3D mode (stub)");
    }

    /// <summary>
    /// Returns to 2D rendering mode.
    /// </summary>
    public void Setup2D()
    {
        ThrowIfDisposed();

        _is3DMode = false;
        _logger.LogDebug("Setup 2D mode");
    }

    /// <summary>
    /// Draws a 3D model.
    /// </summary>
    public void DrawModel(IWmeModel3D model, Matrix4x4 transform)
    {
        ThrowIfDisposed();

        // TODO: Implement 3D model rendering
        // This will be implemented when 3D rendering is added
        _logger.LogDebug("DrawModel called (stub)");
    }

    /// <summary>
    /// Creates a new empty surface.
    /// </summary>
    public IWmeSurface CreateSurface(int width, int height)
    {
        ThrowIfDisposed();

        var surface = new OpenGLSurface(_logger, width, height);
        return surface;
    }

    /// <summary>
    /// Loads a surface from a file.
    /// </summary>
    public IWmeSurface LoadSurface(string filename, Color? colorKey = null)
    {
        ThrowIfDisposed();

        // Check cache first
        if (_surfaceCache.TryGetValue(filename, out var cached))
        {
            _logger.LogDebug("Surface cache hit: {File}", filename);
            return cached;
        }

        // Load from file
        var surface = new OpenGLSurface(_logger, _fileManager, filename, colorKey);
        _surfaceCache[filename] = surface;

        _logger.LogDebug("Loaded surface: {File} ({Width}x{Height})",
            filename, surface.Width, surface.Height);

        return surface;
    }

    /// <summary>
    /// Captures the current frame as an image.
    /// </summary>
    public IWmeImage TakeScreenshot()
    {
        ThrowIfDisposed();

        if (_skiaSurface == null)
            throw new InvalidOperationException("Skia surface not initialized");

        var image = _skiaSurface.Snapshot();
        return new SkiaImage(_logger, image);
    }

    /// <summary>
    /// Performs hit testing.
    /// </summary>
    public object? GetObjectAt(int x, int y)
    {
        ThrowIfDisposed();

        // TODO: Implement hit testing
        // This requires integration with the scene graph
        _logger.LogDebug("GetObjectAt called: ({X}, {Y}) (stub)", x, y);
        return null;
    }

    /// <summary>
    /// Creates a paint object for the specified blend mode and alpha.
    /// </summary>
    private SKPaint CreatePaint(BlendMode blendMode, float alpha)
    {
        var paint = new SKPaint
        {
            IsAntialias = true,
            FilterQuality = SKFilterQuality.High
        };

        // Set alpha
        paint.Color = new SKColor(255, 255, 255, (byte)(alpha * 255));

        // Set blend mode
        paint.BlendMode = blendMode switch
        {
            BlendMode.Normal => SKBlendMode.SrcOver,
            BlendMode.Additive => SKBlendMode.Plus,
            BlendMode.Subtractive => SKBlendMode.Difference,
            BlendMode.Multiply => SKBlendMode.Multiply,
            _ => SKBlendMode.SrcOver
        };

        return paint;
    }

    /// <summary>
    /// Throws if disposed.
    /// </summary>
    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(OpenGLRenderer));
    }

    /// <summary>
    /// Disposes the renderer.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        // Dispose cached surfaces
        foreach (var surface in _surfaceCache.Values)
        {
            surface?.Dispose();
        }
        _surfaceCache.Clear();

        // Dispose Skia resources
        _skiaSurface?.Dispose();
        _grContext?.Dispose();

        // Dispose OpenGL
        _gl?.Dispose();

        // Dispose window
        _window?.Dispose();

        _disposed = true;
        GC.SuppressFinalize(this);

        _logger.LogInformation("OpenGL Renderer disposed");
    }
}
