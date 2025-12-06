namespace WME.Graphics.Images;

using SkiaSharp;

/// <summary>
/// Image implementation using SkiaSharp.
/// </summary>
public class SkiaImage : IWmeImage
{
    private readonly ILogger _logger;
    private SKImage? _image;
    private byte[]? _pixelData;
    private bool _disposed;

    /// <summary>
    /// Gets the image width in pixels.
    /// </summary>
    public int Width { get; private set; }

    /// <summary>
    /// Gets the image height in pixels.
    /// </summary>
    public int Height { get; private set; }

    /// <summary>
    /// Gets the raw pixel data (RGBA format).
    /// </summary>
    public ReadOnlySpan<byte> PixelData
    {
        get
        {
            ThrowIfDisposed();
            return _pixelData ?? ReadOnlySpan<byte>.Empty;
        }
    }

    /// <summary>
    /// Creates an image from an SKImage.
    /// </summary>
    internal SkiaImage(ILogger logger, SKImage image)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _image = image ?? throw new ArgumentNullException(nameof(image));

        Width = image.Width;
        Height = image.Height;

        // Extract pixel data
        ExtractPixelData();
    }

    /// <summary>
    /// Creates an image with specified dimensions and pixel data.
    /// </summary>
    public SkiaImage(ILogger logger, int width, int height, byte[] pixelData)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (width <= 0 || height <= 0)
            throw new ArgumentException("Image dimensions must be positive");

        if (pixelData == null || pixelData.Length != width * height * 4)
            throw new ArgumentException("Invalid pixel data size");

        Width = width;
        Height = height;
        _pixelData = pixelData;

        // Create SKImage from pixel data
        var imageInfo = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var bitmap = new SKBitmap(imageInfo);
        var pixelPtr = bitmap.GetPixels();
        System.Runtime.InteropServices.Marshal.Copy(pixelData, 0, pixelPtr, pixelData.Length);

        _image = SKImage.FromBitmap(bitmap);
    }

    /// <summary>
    /// Extracts pixel data from the SKImage.
    /// </summary>
    private void ExtractPixelData()
    {
        if (_image == null) return;

        var imageInfo = new SKImageInfo(Width, Height, SKColorType.Rgba8888, SKAlphaType.Premul);
        _pixelData = new byte[Width * Height * 4];

        // Read pixels from image
        using var bitmap = SKBitmap.FromImage(_image);
        var pixelPtr = bitmap.GetPixels();
        System.Runtime.InteropServices.Marshal.Copy(pixelPtr, _pixelData, 0, _pixelData.Length);
    }

    /// <summary>
    /// Saves the image to a file.
    /// </summary>
    public async Task<bool> SaveAsync(string filename)
    {
        ThrowIfDisposed();

        if (_image == null)
        {
            _logger.LogError("Cannot save image: image data is null");
            return false;
        }

        try
        {
            // Determine format from extension
            var extension = Path.GetExtension(filename).ToLowerInvariant();
            var format = extension switch
            {
                ".png" => SKEncodedImageFormat.Png,
                ".jpg" or ".jpeg" => SKEncodedImageFormat.Jpeg,
                ".bmp" => SKEncodedImageFormat.Bmp,
                ".webp" => SKEncodedImageFormat.Webp,
                _ => SKEncodedImageFormat.Png
            };

            // Encode and save
            using var data = _image.Encode(format, quality: 100);
            if (data == null)
            {
                _logger.LogError("Failed to encode image: {File}", filename);
                return false;
            }

            await using var fileStream = File.Create(filename);
            data.SaveTo(fileStream);

            _logger.LogInformation("Saved image to: {File} ({Width}x{Height})",
                filename, Width, Height);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save image: {File}", filename);
            return false;
        }
    }

    /// <summary>
    /// Throws if disposed.
    /// </summary>
    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SkiaImage));
    }

    /// <summary>
    /// Disposes the image.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        _image?.Dispose();
        _image = null;
        _pixelData = null;

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
