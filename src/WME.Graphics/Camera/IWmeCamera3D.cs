namespace WME.Graphics.Camera;

using WME.Graphics.Rendering;

/// <summary>
/// Represents a 3D camera for rendering.
/// </summary>
/// <remarks>
/// Full implementation will be added in Phase 2: Graphics System.
/// </remarks>
public interface IWmeCamera3D
{
    /// <summary>
    /// Gets or sets the camera position in 3D space.
    /// </summary>
    Vector3 Position { get; set; }

    /// <summary>
    /// Gets or sets the camera's look-at target.
    /// </summary>
    Vector3 Target { get; set; }

    /// <summary>
    /// Gets or sets the field of view in degrees.
    /// </summary>
    float FieldOfView { get; set; }

    /// <summary>
    /// Gets the view matrix for this camera.
    /// </summary>
    Matrix4x4 ViewMatrix { get; }

    /// <summary>
    /// Gets the projection matrix for this camera.
    /// </summary>
    Matrix4x4 ProjectionMatrix { get; }
}

/// <summary>
/// Represents a 3D vector.
/// </summary>
public record struct Vector3(float X, float Y, float Z)
{
    public static readonly Vector3 Zero = new(0, 0, 0);
    public static readonly Vector3 One = new(1, 1, 1);
    public static readonly Vector3 Up = new(0, 1, 0);
    public static readonly Vector3 Forward = new(0, 0, 1);
}
