namespace WME.Graphics.Surfaces;

using SkiaSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using WME.Core.Files;
using WME.Graphics.Rendering;

/// <summary>
/// OpenGL surface implementation using SkiaSharp.
/// </summary>
public class OpenGLSurface : IWmeSurface
{
    private readonly ILogger _logger;
    private SKImage? _image;
    private bool _disposed;

    /// <summary>
    /// Gets the surface width in pixels.
    /// </summary>
    public int Width { get; private set; }

    /// <summary>
    /// Gets the surface height in pixels.
    /// </summary>
    public int Height { get; private set; }

    /// <summary>
    /// Gets whether this surface has an alpha channel.
    /// </summary>
    public bool HasAlpha { get; private set; }

    /// <summary>
    /// Gets the filename this surface was loaded from.
    /// </summary>
    public string? Filename { get; private set; }

    /// <summary>
    /// Gets the internal SKImage.
    /// </summary>
    internal SKImage Image => _image ?? throw new InvalidOperationException("Surface not initialized");

    /// <summary>
    /// Creates a new empty surface.
    /// </summary>
    public OpenGLSurface(ILogger logger, int width, int height)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (width <= 0 || height <= 0)
            throw new ArgumentException("Surface dimensions must be positive");

        Width = width;
        Height = height;
        HasAlpha = true;

        // Create empty image
        var imageInfo = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var surface = SKSurface.Create(imageInfo);
        surface.Canvas.Clear(SKColors.Transparent);

        _image = surface.Snapshot();
    }

    /// <summary>
    /// Creates a surface from an image file.
    /// </summary>
    public OpenGLSurface(ILogger logger, IWmeFileManager fileManager, string filename, Color? colorKey = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (fileManager == null)
            throw new ArgumentNullException(nameof(fileManager));

        if (string.IsNullOrWhiteSpace(filename))
            throw new ArgumentException("Filename cannot be null or empty", nameof(filename));

        Filename = filename;

        try
        {
            // Load image using file manager
            using var stream = fileManager.OpenFile(filename);
            if (stream == null)
            {
                throw new FileNotFoundException($"Image file not found: {filename}");
            }

            // Load with ImageSharp for better format support
            using var image = SixLabors.ImageSharp.Image.Load<Rgba32>(stream);

            Width = image.Width;
            Height = image.Height;
            HasAlpha = true;

            // Convert to SKImage
            var imageInfo = new SKImageInfo(Width, Height, SKColorType.Rgba8888, SKAlphaType.Premul);
            using var skSurface = SKSurface.Create(imageInfo);
            var canvas = skSurface.Canvas;

            // Copy pixel data
            var pixelData = new byte[Width * Height * 4];
            image.CopyPixelDataTo(pixelData);

            // Apply color key if specified
            if (colorKey.HasValue)
            {
                ApplyColorKey(pixelData, colorKey.Value);
            }

            // Create SKBitmap and draw to surface
            using var bitmap = new SKBitmap(Width, Height, SKColorType.Rgba8888, SKAlphaType.Premul);
            var pixelPtr = bitmap.GetPixels();
            System.Runtime.InteropServices.Marshal.Copy(pixelData, 0, pixelPtr, pixelData.Length);

            canvas.DrawBitmap(bitmap, 0, 0);

            _image = skSurface.Snapshot();

            _logger.LogDebug("Loaded surface from file: {File} ({Width}x{Height})",
                filename, Width, Height);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load surface from file: {File}", filename);
            throw;
        }
    }

    /// <summary>
    /// Applies a color key (transparency) to pixel data.
    /// </summary>
    private void ApplyColorKey(byte[] pixelData, Color colorKey)
    {
        for (int i = 0; i < pixelData.Length; i += 4)
        {
            var r = pixelData[i];
            var g = pixelData[i + 1];
            var b = pixelData[i + 2];

            // If pixel matches color key, make it transparent
            if (r == colorKey.R && g == colorKey.G && b == colorKey.B)
            {
                pixelData[i + 3] = 0; // Set alpha to 0
            }
        }
    }

    /// <summary>
    /// Throws if disposed.
    /// </summary>
    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(OpenGLSurface));
    }

    /// <summary>
    /// Disposes the surface.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        _image?.Dispose();
        _image = null;

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
