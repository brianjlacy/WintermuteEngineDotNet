namespace WME.Audio.Sound;

/// <summary>
/// Core audio manager interface for Wintermute Engine.
/// Manages sound playback, volume control, and audio resource lifecycle.
/// </summary>
public interface IWmeSoundManager : IDisposable
{
    /// <summary>
    /// Initializes the audio system.
    /// </summary>
    /// <returns>Task representing the async initialization operation.</returns>
    Task InitializeAsync();

    /// <summary>
    /// Loads a sound file and creates a sound buffer.
    /// </summary>
    /// <param name="filename">Path to the audio file.</param>
    /// <param name="type">Type of sound (SFX, Music, Speech).</param>
    /// <param name="streamed">If true, stream from disk instead of loading into memory.</param>
    /// <returns>The loaded sound buffer, or null if loading failed.</returns>
    IWmeSoundBuffer? LoadSound(string filename, SoundType type, bool streamed = false);

    /// <summary>
    /// Unloads a sound buffer and releases its resources.
    /// </summary>
    /// <param name="sound">The sound buffer to unload.</param>
    void UnloadSound(IWmeSoundBuffer sound);

    /// <summary>
    /// Sets the master volume for all sounds.
    /// </summary>
    /// <param name="volume">Volume level (0.0 = silent, 1.0 = full volume).</param>
    void SetMasterVolume(float volume);

    /// <summary>
    /// Sets the volume for a specific sound type.
    /// </summary>
    /// <param name="type">The sound type to adjust.</param>
    /// <param name="volume">Volume level (0.0 = silent, 1.0 = full volume).</param>
    void SetTypeVolume(SoundType type, float volume);

    /// <summary>
    /// Gets the current volume for a specific sound type.
    /// </summary>
    /// <param name="type">The sound type to query.</param>
    /// <returns>The current volume level.</returns>
    float GetTypeVolume(SoundType type);

    /// <summary>
    /// Pauses all currently playing sounds.
    /// </summary>
    /// <param name="includingMusic">If true, also pauses music; otherwise only SFX and speech.</param>
    void PauseAll(bool includingMusic = false);

    /// <summary>
    /// Resumes all paused sounds.
    /// </summary>
    void ResumeAll();

    /// <summary>
    /// Stops all currently playing sounds.
    /// </summary>
    void StopAll();

    /// <summary>
    /// Updates the audio system. Should be called once per frame.
    /// </summary>
    /// <param name="deltaTime">Time elapsed since last update.</param>
    void Update(TimeSpan deltaTime);
}

/// <summary>
/// Represents a sound buffer (loaded audio data).
/// </summary>
public interface IWmeSoundBuffer : IDisposable
{
    /// <summary>
    /// Gets the type of this sound (SFX, Music, or Speech).
    /// </summary>
    SoundType Type { get; }

    /// <summary>
    /// Gets the current playback state.
    /// </summary>
    SoundState State { get; }

    /// <summary>
    /// Gets or sets whether this sound should loop.
    /// </summary>
    bool IsLooping { get; set; }

    /// <summary>
    /// Gets or sets the volume for this specific sound (0.0 to 1.0).
    /// </summary>
    float Volume { get; set; }

    /// <summary>
    /// Gets or sets the stereo pan (-1.0 = left, 0.0 = center, 1.0 = right).
    /// </summary>
    float Pan { get; set; }

    /// <summary>
    /// Gets or sets the current playback position.
    /// </summary>
    TimeSpan Position { get; set; }

    /// <summary>
    /// Gets the total duration of the sound.
    /// </summary>
    TimeSpan Duration { get; }

    /// <summary>
    /// Starts or resumes playback of this sound.
    /// </summary>
    /// <param name="loop">If true, the sound will loop indefinitely.</param>
    void Play(bool loop = false);

    /// <summary>
    /// Pauses playback of this sound.
    /// </summary>
    void Pause();

    /// <summary>
    /// Resumes playback of a paused sound.
    /// </summary>
    void Resume();

    /// <summary>
    /// Stops playback and resets the position to the beginning.
    /// </summary>
    void Stop();
}

/// <summary>
/// Categorizes sounds for volume control and management.
/// </summary>
public enum SoundType
{
    /// <summary>
    /// Sound effects (footsteps, door opens, etc.).
    /// </summary>
    SFX,

    /// <summary>
    /// Background music.
    /// </summary>
    Music,

    /// <summary>
    /// Character dialogue and voice-over.
    /// </summary>
    Speech
}

/// <summary>
/// Represents the playback state of a sound.
/// </summary>
public enum SoundState
{
    /// <summary>
    /// Sound is not playing.
    /// </summary>
    Stopped,

    /// <summary>
    /// Sound is currently playing.
    /// </summary>
    Playing,

    /// <summary>
    /// Sound is paused (can be resumed).
    /// </summary>
    Paused
}
