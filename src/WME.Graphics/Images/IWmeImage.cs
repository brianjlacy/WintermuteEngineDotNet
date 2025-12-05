namespace WME.Graphics.Images;

/// <summary>
/// Represents an image (CPU-side pixel data).
/// </summary>
public interface IWmeImage : IDisposable
{
    /// <summary>
    /// Gets the image width in pixels.
    /// </summary>
    int Width { get; }

    /// <summary>
    /// Gets the image height in pixels.
    /// </summary>
    int Height { get; }

    /// <summary>
    /// Gets the raw pixel data (RGBA format).
    /// </summary>
    ReadOnlySpan<byte> PixelData { get; }

    /// <summary>
    /// Saves the image to a file.
    /// </summary>
    /// <param name="filename">Output filename.</param>
    /// <returns>True if save succeeded.</returns>
    Task<bool> SaveAsync(string filename);
}
