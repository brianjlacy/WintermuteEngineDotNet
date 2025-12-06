namespace WME.Audio.Sound;

using Silk.NET.OpenAL;
using NAudio.Wave;
using System.IO;

/// <summary>
/// OpenAL sound buffer implementation.
/// </summary>
public class OpenALSoundBuffer : IWmeSoundBuffer
{
    private readonly ILogger _logger;
    private readonly AL _al;
    private readonly IWmeFileManager _fileManager;
    private readonly string _filename;

    private uint _source;
    private uint _buffer;
    private bool _disposed;
    private SoundState _state = SoundState.Stopped;
    private float _volume = 1.0f;
    private float _pan = 0.0f;
    private bool _isLooping;

    /// <summary>
    /// Gets the type of this sound.
    /// </summary>
    public SoundType Type { get; }

    /// <summary>
    /// Gets whether this is a streaming sound.
    /// </summary>
    public bool IsStreaming { get; }

    /// <summary>
    /// Gets the current playback state.
    /// </summary>
    public SoundState State
    {
        get
        {
            UpdateState();
            return _state;
        }
    }

    /// <summary>
    /// Gets or sets whether this sound should loop.
    /// </summary>
    public bool IsLooping
    {
        get => _isLooping;
        set
        {
            ThrowIfDisposed();
            _isLooping = value;

            if (_source != 0)
            {
                _al.SetSourceProperty(_source, SourceBoolean.Looping, value);
            }
        }
    }

    /// <summary>
    /// Gets or sets the volume.
    /// </summary>
    public float Volume
    {
        get => _volume;
        set
        {
            ThrowIfDisposed();
            _volume = Math.Clamp(value, 0.0f, 1.0f);

            if (_source != 0)
            {
                _al.SetSourceProperty(_source, SourceFloat.Gain, _volume);
            }
        }
    }

    /// <summary>
    /// Gets or sets the stereo pan.
    /// </summary>
    public float Pan
    {
        get => _pan;
        set
        {
            ThrowIfDisposed();
            _pan = Math.Clamp(value, -1.0f, 1.0f);

            if (_source != 0)
            {
                // Convert pan to 3D position (simple stereo panning)
                _al.SetSourceProperty(_source, SourceVector3.Position, _pan, 0, 0);
            }
        }
    }

    /// <summary>
    /// Gets or sets the current playback position.
    /// </summary>
    public TimeSpan Position
    {
        get
        {
            ThrowIfDisposed();

            if (_source == 0)
                return TimeSpan.Zero;

            _al.GetSourceProperty(_source, GetSourceInteger.SampleOffset, out int sampleOffset);
            _al.GetSourceProperty(_source, GetSourceInteger.Frequency, out int frequency);

            if (frequency > 0)
            {
                return TimeSpan.FromSeconds((double)sampleOffset / frequency);
            }

            return TimeSpan.Zero;
        }
        set
        {
            ThrowIfDisposed();

            if (_source != 0)
            {
                _al.GetSourceProperty(_source, GetSourceInteger.Frequency, out int frequency);
                if (frequency > 0)
                {
                    int sampleOffset = (int)(value.TotalSeconds * frequency);
                    _al.SetSourceProperty(_source, SourceInteger.SampleOffset, sampleOffset);
                }
            }
        }
    }

    /// <summary>
    /// Gets the total duration of the sound.
    /// </summary>
    public TimeSpan Duration { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenALSoundBuffer"/> class.
    /// </summary>
    public OpenALSoundBuffer(
        ILogger logger,
        AL al,
        IWmeFileManager fileManager,
        string filename,
        SoundType type,
        bool streamed)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _al = al ?? throw new ArgumentNullException(nameof(al));
        _fileManager = fileManager ?? throw new ArgumentNullException(nameof(fileManager));
        _filename = filename ?? throw new ArgumentNullException(nameof(filename));

        Type = type;
        IsStreaming = streamed;

        // Load the sound
        LoadSound();
    }

    /// <summary>
    /// Loads the sound data.
    /// </summary>
    private void LoadSound()
    {
        try
        {
            using var stream = _fileManager.OpenFile(_filename);
            if (stream == null)
            {
                throw new FileNotFoundException($"Sound file not found: {_filename}");
            }

            // Read audio data using NAudio
            using var reader = new WaveFileReader(stream);
            var waveFormat = reader.WaveFormat;

            // Read all samples
            var samples = new byte[reader.Length];
            reader.Read(samples, 0, samples.Length);

            // Determine OpenAL format
            BufferFormat format;
            if (waveFormat.Channels == 1)
            {
                format = waveFormat.BitsPerSample == 8
                    ? BufferFormat.Mono8
                    : BufferFormat.Mono16;
            }
            else
            {
                format = waveFormat.BitsPerSample == 8
                    ? BufferFormat.Stereo8
                    : BufferFormat.Stereo16;
            }

            // Create OpenAL buffer
            _buffer = _al.GenBuffer();
            _al.BufferData(_buffer, format, samples, waveFormat.SampleRate);

            // Create OpenAL source
            _source = _al.GenSource();
            _al.SetSourceProperty(_source, SourceInteger.Buffer, _buffer);

            // Calculate duration
            Duration = TimeSpan.FromSeconds((double)samples.Length / waveFormat.AverageBytesPerSecond);

            _logger.LogDebug("Loaded sound: {File} ({Duration}ms, {Channels}ch, {Rate}Hz)",
                _filename, Duration.TotalMilliseconds, waveFormat.Channels, waveFormat.SampleRate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load sound: {File}", _filename);
            throw;
        }
    }

    /// <summary>
    /// Starts or resumes playback.
    /// </summary>
    public void Play(bool loop = false)
    {
        ThrowIfDisposed();

        if (_source == 0)
            return;

        IsLooping = loop;
        _al.SourcePlay(_source);
        _state = SoundState.Playing;

        _logger.LogDebug("Playing sound: {File} (Loop: {Loop})", _filename, loop);
    }

    /// <summary>
    /// Pauses playback.
    /// </summary>
    public void Pause()
    {
        ThrowIfDisposed();

        if (_source == 0 || _state != SoundState.Playing)
            return;

        _al.SourcePause(_source);
        _state = SoundState.Paused;

        _logger.LogDebug("Paused sound: {File}", _filename);
    }

    /// <summary>
    /// Resumes playback.
    /// </summary>
    public void Resume()
    {
        ThrowIfDisposed();

        if (_source == 0 || _state != SoundState.Paused)
            return;

        _al.SourcePlay(_source);
        _state = SoundState.Playing;

        _logger.LogDebug("Resumed sound: {File}", _filename);
    }

    /// <summary>
    /// Stops playback.
    /// </summary>
    public void Stop()
    {
        ThrowIfDisposed();

        if (_source == 0)
            return;

        _al.SourceStop(_source);
        _al.SetSourceProperty(_source, SourceInteger.SampleOffset, 0);
        _state = SoundState.Stopped;

        _logger.LogDebug("Stopped sound: {File}", _filename);
    }

    /// <summary>
    /// Updates the effective volume based on master and type volumes.
    /// </summary>
    internal void UpdateEffectiveVolume(float masterVolume, float typeVolume)
    {
        if (_source != 0)
        {
            var effectiveVolume = _volume * masterVolume * typeVolume;
            _al.SetSourceProperty(_source, SourceFloat.Gain, effectiveVolume);
        }
    }

    /// <summary>
    /// Updates streaming playback (for streamed sounds).
    /// </summary>
    internal void UpdateStreaming()
    {
        // TODO: Implement buffer queuing for streaming
        // This would involve:
        // 1. Checking processed buffers
        // 2. Unqueuing processed buffers
        // 3. Loading next chunk of audio data
        // 4. Queuing new buffers
    }

    /// <summary>
    /// Updates the playback state from OpenAL.
    /// </summary>
    private void UpdateState()
    {
        if (_source == 0)
        {
            _state = SoundState.Stopped;
            return;
        }

        _al.GetSourceProperty(_source, GetSourceInteger.SourceState, out int state);

        _state = (SourceState)state switch
        {
            SourceState.Playing => SoundState.Playing,
            SourceState.Paused => SoundState.Paused,
            _ => SoundState.Stopped
        };
    }

    /// <summary>
    /// Throws if disposed.
    /// </summary>
    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(OpenALSoundBuffer));
    }

    /// <summary>
    /// Disposes the sound buffer.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        // Stop playback
        if (_source != 0)
        {
            _al.SourceStop(_source);
            _al.DeleteSource(_source);
            _source = 0;
        }

        // Delete buffer
        if (_buffer != 0)
        {
            _al.DeleteBuffer(_buffer);
            _buffer = 0;
        }

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
