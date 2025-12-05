namespace WME.Graphics.Models;

/// <summary>
/// Represents a 3D model for rendering.
/// </summary>
/// <remarks>
/// Full implementation will be added in Phase 2: Graphics System.
/// </remarks>
public interface IWmeModel3D : IDisposable
{
    /// <summary>
    /// Gets the model filename.
    /// </summary>
    string? Filename { get; }

    /// <summary>
    /// Gets whether the model has been loaded successfully.
    /// </summary>
    bool IsLoaded { get; }
}
