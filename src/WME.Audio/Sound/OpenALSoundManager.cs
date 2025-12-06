namespace WME.Audio.Sound;

using Silk.NET.OpenAL;
using System.Collections.Concurrent;
using WME.Core.Files;

/// <summary>
/// OpenAL-based sound manager implementation.
/// </summary>
public class OpenALSoundManager : IWmeSoundManager
{
    private readonly ILogger<OpenALSoundManager> _logger;
    private readonly IWmeFileManager _fileManager;
    private readonly ConcurrentDictionary<string, IWmeSoundBuffer> _loadedSounds = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<IWmeSoundBuffer> _activeSounds = new();

    private AL? _al;
    private ALContext? _alc;
    private unsafe Device* _device;
    private unsafe Context* _context;

    private float _masterVolume = 1.0f;
    private readonly Dictionary<SoundType, float> _typeVolumes = new()
    {
        { SoundType.SFX, 1.0f },
        { SoundType.Music, 0.8f },
        { SoundType.Speech, 1.0f }
    };

    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenALSoundManager"/> class.
    /// </summary>
    public OpenALSoundManager(ILogger<OpenALSoundManager> logger, IWmeFileManager fileManager)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _fileManager = fileManager ?? throw new ArgumentNullException(nameof(fileManager));
    }

    /// <summary>
    /// Initializes the audio system.
    /// </summary>
    public unsafe Task InitializeAsync()
    {
        ThrowIfDisposed();

        return Task.Run(() =>
        {
            try
            {
                // Initialize OpenAL
                _al = AL.GetApi();
                _alc = ALContext.GetApi();

                // Open default device
                _device = _alc.OpenDevice(string.Empty);
                if (_device == null)
                {
                    throw new InvalidOperationException("Failed to open OpenAL device");
                }

                // Create context
                _context = _alc.CreateContext(_device, null);
                if (_context == null)
                {
                    throw new InvalidOperationException("Failed to create OpenAL context");
                }

                // Make context current
                _alc.MakeContextCurrent(_context);

                // Set listener properties
                _al.SetListenerProperty(ListenerVector3.Position, 0, 0, 0);
                _al.SetListenerProperty(ListenerVector3.Velocity, 0, 0, 0);

                float[] orientation = { 0, 0, -1, 0, 1, 0 }; // Forward and up vectors
                _al.SetListenerProperty(ListenerFloatArray.Orientation, orientation);

                _logger.LogInformation("OpenAL initialized successfully");
                LogAudioDeviceInfo();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize OpenAL");
                throw;
            }
        });
    }

    /// <summary>
    /// Logs audio device information.
    /// </summary>
    private unsafe void LogAudioDeviceInfo()
    {
        if (_alc == null || _device == null) return;

        var deviceName = _alc.GetContextProperty(_device, GetContextString.DeviceSpecifier);
        var vendor = _al?.GetStateProperty(StateString.Vendor);
        var version = _al?.GetStateProperty(StateString.Version);
        var renderer = _al?.GetStateProperty(StateString.Renderer);

        _logger.LogInformation("OpenAL Device: {Device}", deviceName);
        _logger.LogInformation("OpenAL Vendor: {Vendor}", vendor);
        _logger.LogInformation("OpenAL Version: {Version}", version);
        _logger.LogInformation("OpenAL Renderer: {Renderer}", renderer);
    }

    /// <summary>
    /// Loads a sound file and creates a sound buffer.
    /// </summary>
    public IWmeSoundBuffer? LoadSound(string filename, SoundType type, bool streamed = false)
    {
        ThrowIfDisposed();

        if (string.IsNullOrWhiteSpace(filename))
            throw new ArgumentException("Filename cannot be null or empty", nameof(filename));

        // Check cache
        if (_loadedSounds.TryGetValue(filename, out var cached))
        {
            _logger.LogDebug("Sound cache hit: {File}", filename);
            return cached;
        }

        try
        {
            _logger.LogDebug("Loading sound: {File} (Type: {Type}, Streamed: {Streamed})",
                filename, type, streamed);

            var sound = new OpenALSoundBuffer(_logger, _al!, _fileManager, filename, type, streamed);
            _loadedSounds[filename] = sound;
            _activeSounds.Add(sound);

            return sound;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load sound: {File}", filename);
            return null;
        }
    }

    /// <summary>
    /// Unloads a sound buffer.
    /// </summary>
    public void UnloadSound(IWmeSoundBuffer sound)
    {
        ThrowIfDisposed();

        if (sound == null)
            return;

        _activeSounds.Remove(sound);

        var entry = _loadedSounds.FirstOrDefault(kvp => kvp.Value == sound);
        if (entry.Key != null)
        {
            _loadedSounds.TryRemove(entry.Key, out _);
        }

        sound.Dispose();
        _logger.LogDebug("Unloaded sound");
    }

    /// <summary>
    /// Sets the master volume.
    /// </summary>
    public void SetMasterVolume(float volume)
    {
        ThrowIfDisposed();

        _masterVolume = Math.Clamp(volume, 0.0f, 1.0f);

        // Update all active sounds
        foreach (var sound in _activeSounds.ToList())
        {
            if (sound is OpenALSoundBuffer openALSound)
            {
                openALSound.UpdateEffectiveVolume(_masterVolume, _typeVolumes[sound.Type]);
            }
        }

        _logger.LogDebug("Master volume set to: {Volume}", _masterVolume);
    }

    /// <summary>
    /// Sets the volume for a specific sound type.
    /// </summary>
    public void SetTypeVolume(SoundType type, float volume)
    {
        ThrowIfDisposed();

        volume = Math.Clamp(volume, 0.0f, 1.0f);
        _typeVolumes[type] = volume;

        // Update sounds of this type
        foreach (var sound in _activeSounds.Where(s => s.Type == type).ToList())
        {
            if (sound is OpenALSoundBuffer openALSound)
            {
                openALSound.UpdateEffectiveVolume(_masterVolume, volume);
            }
        }

        _logger.LogDebug("{Type} volume set to: {Volume}", type, volume);
    }

    /// <summary>
    /// Gets the volume for a specific sound type.
    /// </summary>
    public float GetTypeVolume(SoundType type)
    {
        ThrowIfDisposed();
        return _typeVolumes.TryGetValue(type, out var volume) ? volume : 1.0f;
    }

    /// <summary>
    /// Pauses all currently playing sounds.
    /// </summary>
    public void PauseAll(bool includingMusic = false)
    {
        ThrowIfDisposed();

        foreach (var sound in _activeSounds.ToList())
        {
            if (!includingMusic && sound.Type == SoundType.Music)
                continue;

            if (sound.State == SoundState.Playing)
            {
                sound.Pause();
            }
        }

        _logger.LogDebug("Paused all sounds (including music: {IncludingMusic})", includingMusic);
    }

    /// <summary>
    /// Resumes all paused sounds.
    /// </summary>
    public void ResumeAll()
    {
        ThrowIfDisposed();

        foreach (var sound in _activeSounds.ToList())
        {
            if (sound.State == SoundState.Paused)
            {
                sound.Resume();
            }
        }

        _logger.LogDebug("Resumed all sounds");
    }

    /// <summary>
    /// Stops all currently playing sounds.
    /// </summary>
    public void StopAll()
    {
        ThrowIfDisposed();

        foreach (var sound in _activeSounds.ToList())
        {
            sound.Stop();
        }

        _logger.LogDebug("Stopped all sounds");
    }

    /// <summary>
    /// Updates the audio system.
    /// </summary>
    public void Update(TimeSpan deltaTime)
    {
        ThrowIfDisposed();

        // Update streaming sounds
        foreach (var sound in _activeSounds.ToList())
        {
            if (sound is OpenALSoundBuffer openALSound && openALSound.IsStreaming)
            {
                openALSound.UpdateStreaming();
            }
        }

        // Remove finished non-looping sounds
        _activeSounds.RemoveAll(s => s.State == SoundState.Stopped && !s.IsLooping);
    }

    /// <summary>
    /// Throws if disposed.
    /// </summary>
    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(OpenALSoundManager));
    }

    /// <summary>
    /// Disposes the sound manager.
    /// </summary>
    public unsafe void Dispose()
    {
        if (_disposed) return;

        // Stop and dispose all sounds
        foreach (var sound in _loadedSounds.Values)
        {
            sound?.Dispose();
        }

        _loadedSounds.Clear();
        _activeSounds.Clear();

        // Cleanup OpenAL
        if (_alc != null && _context != null)
        {
            _alc.MakeContextCurrent(null);
            _alc.DestroyContext(_context);
        }

        if (_alc != null && _device != null)
        {
            _alc.CloseDevice(_device);
        }

        _alc?.Dispose();
        _al?.Dispose();

        _disposed = true;
        GC.SuppressFinalize(this);

        _logger.LogInformation("OpenAL Sound Manager disposed");
    }
}
