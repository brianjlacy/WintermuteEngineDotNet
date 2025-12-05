namespace WME.Core.Scripting;

/// <summary>
/// Represents a compiled and running script.
/// </summary>
/// <remarks>
/// Full implementation will be added in Phase 4: Script System.
/// </remarks>
public class WmeScript : IDisposable
{
    /// <summary>
    /// Gets or sets the script filename.
    /// </summary>
    public string? Filename { get; set; }

    /// <summary>
    /// Gets the current state of the script.
    /// </summary>
    public ScriptState State { get; set; }

    /// <summary>
    /// Releases resources used by this script.
    /// </summary>
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Represents the execution state of a script.
/// </summary>
public enum ScriptState
{
    /// <summary>
    /// Script is waiting to run.
    /// </summary>
    Waiting,

    /// <summary>
    /// Script is currently running.
    /// </summary>
    Running,

    /// <summary>
    /// Script is sleeping (delayed execution).
    /// </summary>
    Sleeping,

    /// <summary>
    /// Script has finished execution.
    /// </summary>
    Finished,

    /// <summary>
    /// Script has been paused.
    /// </summary>
    Paused
}
