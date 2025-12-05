namespace WME.Core.Game;

/// <summary>
/// Main game interface for Wintermute Engine.
/// Manages game initialization, update loop, rendering, and subsystem coordination.
/// </summary>
public interface IWmeGame : IDisposable
{
    /// <summary>
    /// Gets the current game state (Running, Frozen, SemiFrozen).
    /// </summary>
    GameState State { get; }

    /// <summary>
    /// Gets or sets the currently active object (receives input events).
    /// </summary>
    WmeObject? ActiveObject { get; set; }

    /// <summary>
    /// Gets or sets the main game object (typically the active scene or game mode).
    /// </summary>
    WmeObject? MainObject { get; set; }

    /// <summary>
    /// Gets the collection of UI windows.
    /// </summary>
    IReadOnlyList<WmeWindow> Windows { get; }

    /// <summary>
    /// Gets the collection of running scripts.
    /// </summary>
    IReadOnlyList<WmeScript> Scripts { get; }

    /// <summary>
    /// Gets the total elapsed game time.
    /// </summary>
    TimeSpan GameTime { get; }

    /// <summary>
    /// Gets the time elapsed since the last frame.
    /// </summary>
    TimeSpan DeltaTime { get; }

    /// <summary>
    /// Gets or sets the target frames per second.
    /// </summary>
    int TargetFPS { get; set; }

    /// <summary>
    /// Gets the engine version.
    /// </summary>
    Version EngineVersion { get; }

    /// <summary>
    /// Gets the save game format version.
    /// </summary>
    int SaveGameVersion { get; }

    /// <summary>
    /// Initializes the game engine and all subsystems.
    /// </summary>
    /// <param name="projectFile">Path to the project configuration file.</param>
    /// <returns>True if initialization succeeded, false otherwise.</returns>
    Task<bool> InitializeAsync(string projectFile);

    /// <summary>
    /// Updates game logic for the current frame.
    /// </summary>
    /// <param name="deltaTime">Time elapsed since the last update.</param>
    void Update(TimeSpan deltaTime);

    /// <summary>
    /// Renders the current frame.
    /// </summary>
    void Render();

    /// <summary>
    /// Loads a project configuration file.
    /// </summary>
    /// <param name="projectFile">Path to the project file.</param>
    /// <returns>True if the project was loaded successfully.</returns>
    Task<bool> LoadProjectAsync(string projectFile);

    /// <summary>
    /// Saves the current game state.
    /// </summary>
    /// <param name="filename">Path to save file.</param>
    /// <param name="description">Save game description.</param>
    /// <returns>True if save succeeded.</returns>
    Task<bool> SaveGameAsync(string filename, string description);

    /// <summary>
    /// Loads a saved game state.
    /// </summary>
    /// <param name="filename">Path to save file.</param>
    /// <returns>True if load succeeded.</returns>
    Task<bool> LoadGameAsync(string filename);
}

/// <summary>
/// Represents the current state of the game engine.
/// </summary>
public enum GameState
{
    /// <summary>
    /// Game is running normally.
    /// </summary>
    Running,

    /// <summary>
    /// Game is completely frozen (paused, no updates).
    /// </summary>
    Frozen,

    /// <summary>
    /// Game is semi-frozen (scripts run, but animations are paused).
    /// </summary>
    SemiFrozen
}
