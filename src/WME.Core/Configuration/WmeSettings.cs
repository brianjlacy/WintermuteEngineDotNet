namespace WME.Core.Configuration;

using System.Collections.Concurrent;
using System.Text.Json;

/// <summary>
/// Settings manager implementation using JSON storage.
/// Modern cross-platform replacement for CBRegistry from original WME.
/// </summary>
public class WmeSettings : IWmeSettings
{
    private readonly ILogger<WmeSettings> _logger;
    private readonly ConcurrentDictionary<string, Dictionary<string, object>> _settings = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _fileLock = new object();
    private bool _disposed;
    private bool _isDirty;

    /// <summary>
    /// Gets or sets the configuration file path.
    /// </summary>
    public string ConfigFilePath { get; set; }

    /// <summary>
    /// Gets or sets the base section/category for settings.
    /// </summary>
    public string BasePath { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="WmeSettings"/> class.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    /// <param name="configFilePath">Path to configuration file (defaults to wme.json).</param>
    public WmeSettings(ILogger<WmeSettings> logger, string? configFilePath = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        ConfigFilePath = configFilePath ?? Path.Combine(Environment.CurrentDirectory, "wme.json");
        BasePath = "WintermuteEngine";
        _isDirty = false;

        // Attempt to load existing settings
        Load();
    }

    /// <summary>
    /// Reads a string value from settings.
    /// </summary>
    public string ReadString(string section, string key, string? defaultValue = null)
    {
        ThrowIfDisposed();

        var fullSection = GetFullSection(section);
        if (_settings.TryGetValue(fullSection, out var sectionDict) &&
            sectionDict.TryGetValue(key, out var value))
        {
            return value?.ToString() ?? defaultValue ?? string.Empty;
        }

        return defaultValue ?? string.Empty;
    }

    /// <summary>
    /// Writes a string value to settings.
    /// </summary>
    public bool WriteString(string section, string key, string value)
    {
        ThrowIfDisposed();

        try
        {
            var fullSection = GetFullSection(section);
            var sectionDict = _settings.GetOrAdd(fullSection, _ => new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase));
            sectionDict[key] = value;
            _isDirty = true;
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write string setting: {Section}.{Key}", section, key);
            return false;
        }
    }

    /// <summary>
    /// Reads an integer value from settings.
    /// </summary>
    public int ReadInt(string section, string key, int defaultValue = 0)
    {
        ThrowIfDisposed();

        var fullSection = GetFullSection(section);
        if (_settings.TryGetValue(fullSection, out var sectionDict) &&
            sectionDict.TryGetValue(key, out var value))
        {
            if (value is JsonElement jsonElement)
            {
                if (jsonElement.TryGetInt32(out var intValue))
                    return intValue;
            }
            else if (value != null)
            {
                try
                {
                    return Convert.ToInt32(value);
                }
                catch
                {
                    // Fall through to default
                }
            }
        }

        return defaultValue;
    }

    /// <summary>
    /// Writes an integer value to settings.
    /// </summary>
    public bool WriteInt(string section, string key, int value)
    {
        ThrowIfDisposed();

        try
        {
            var fullSection = GetFullSection(section);
            var sectionDict = _settings.GetOrAdd(fullSection, _ => new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase));
            sectionDict[key] = value;
            _isDirty = true;
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write int setting: {Section}.{Key}", section, key);
            return false;
        }
    }

    /// <summary>
    /// Reads a boolean value from settings.
    /// </summary>
    public bool ReadBool(string section, string key, bool defaultValue = false)
    {
        ThrowIfDisposed();

        var fullSection = GetFullSection(section);
        if (_settings.TryGetValue(fullSection, out var sectionDict) &&
            sectionDict.TryGetValue(key, out var value))
        {
            if (value is JsonElement jsonElement)
            {
                if (jsonElement.ValueKind == JsonValueKind.True) return true;
                if (jsonElement.ValueKind == JsonValueKind.False) return false;
            }
            else if (value is bool boolValue)
            {
                return boolValue;
            }
            else if (value != null)
            {
                try
                {
                    return Convert.ToBoolean(value);
                }
                catch
                {
                    // Fall through to default
                }
            }
        }

        return defaultValue;
    }

    /// <summary>
    /// Writes a boolean value to settings.
    /// </summary>
    public bool WriteBool(string section, string key, bool value)
    {
        ThrowIfDisposed();

        try
        {
            var fullSection = GetFullSection(section);
            var sectionDict = _settings.GetOrAdd(fullSection, _ => new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase));
            sectionDict[key] = value;
            _isDirty = true;
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write bool setting: {Section}.{Key}", section, key);
            return false;
        }
    }

    /// <summary>
    /// Reads a floating-point value from settings.
    /// </summary>
    public float ReadFloat(string section, string key, float defaultValue = 0.0f)
    {
        ThrowIfDisposed();

        var fullSection = GetFullSection(section);
        if (_settings.TryGetValue(fullSection, out var sectionDict) &&
            sectionDict.TryGetValue(key, out var value))
        {
            if (value is JsonElement jsonElement)
            {
                if (jsonElement.TryGetSingle(out var floatValue))
                    return floatValue;
                if (jsonElement.TryGetDouble(out var doubleValue))
                    return (float)doubleValue;
            }
            else if (value != null)
            {
                try
                {
                    return Convert.ToSingle(value);
                }
                catch
                {
                    // Fall through to default
                }
            }
        }

        return defaultValue;
    }

    /// <summary>
    /// Writes a floating-point value to settings.
    /// </summary>
    public bool WriteFloat(string section, string key, float value)
    {
        ThrowIfDisposed();

        try
        {
            var fullSection = GetFullSection(section);
            var sectionDict = _settings.GetOrAdd(fullSection, _ => new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase));
            sectionDict[key] = value;
            _isDirty = true;
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write float setting: {Section}.{Key}", section, key);
            return false;
        }
    }

    /// <summary>
    /// Checks if a setting key exists.
    /// </summary>
    public bool HasKey(string section, string key)
    {
        ThrowIfDisposed();

        var fullSection = GetFullSection(section);
        return _settings.TryGetValue(fullSection, out var sectionDict) &&
               sectionDict.ContainsKey(key);
    }

    /// <summary>
    /// Removes a setting key.
    /// </summary>
    public bool RemoveKey(string section, string key)
    {
        ThrowIfDisposed();

        try
        {
            var fullSection = GetFullSection(section);
            if (_settings.TryGetValue(fullSection, out var sectionDict))
            {
                if (sectionDict.Remove(key))
                {
                    _isDirty = true;
                    return true;
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove setting: {Section}.{Key}", section, key);
            return false;
        }
    }

    /// <summary>
    /// Loads settings from file.
    /// </summary>
    public bool Load()
    {
        try
        {
            lock (_fileLock)
            {
                if (!File.Exists(ConfigFilePath))
                {
                    _logger.LogInformation("Configuration file not found, using defaults: {Path}", ConfigFilePath);
                    LoadDefaults();
                    return true;
                }

                var json = File.ReadAllText(ConfigFilePath);
                var loadedSettings = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, object>>>(json);

                if (loadedSettings != null)
                {
                    _settings.Clear();
                    foreach (var section in loadedSettings)
                    {
                        _settings[section.Key] = section.Value;
                    }

                    _logger.LogInformation("Loaded configuration from: {Path}", ConfigFilePath);
                    _isDirty = false;
                    return true;
                }

                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load configuration from: {Path}", ConfigFilePath);
            LoadDefaults();
            return false;
        }
    }

    /// <summary>
    /// Saves settings to file.
    /// </summary>
    public bool Save()
    {
        ThrowIfDisposed();

        if (!_isDirty)
        {
            _logger.LogDebug("Settings unchanged, skipping save");
            return true;
        }

        try
        {
            lock (_fileLock)
            {
                var directory = Path.GetDirectoryName(ConfigFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                var json = JsonSerializer.Serialize(_settings.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value), options);

                File.WriteAllText(ConfigFilePath, json);

                _logger.LogInformation("Saved configuration to: {Path}", ConfigFilePath);
                _isDirty = false;
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save configuration to: {Path}", ConfigFilePath);
            return false;
        }
    }

    /// <summary>
    /// Resets all settings to defaults.
    /// </summary>
    public void Clear()
    {
        ThrowIfDisposed();

        _settings.Clear();
        LoadDefaults();
        _isDirty = true;
    }

    /// <summary>
    /// Loads default settings.
    /// </summary>
    private void LoadDefaults()
    {
        _logger.LogDebug("Loading default settings");

        // Video defaults
        WriteInt("Video", "Width", 1920);
        WriteInt("Video", "Height", 1080);
        WriteBool("Video", "Windowed", true);
        WriteBool("Video", "VSync", true);
        WriteInt("Video", "MSAA", 0);

        // Audio defaults
        WriteFloat("Audio", "MasterVolume", 1.0f);
        WriteFloat("Audio", "MusicVolume", 0.8f);
        WriteFloat("Audio", "SFXVolume", 1.0f);
        WriteFloat("Audio", "SpeechVolume", 1.0f);

        // Engine defaults
        WriteInt("Engine", "TargetFPS", 60);
        WriteBool("Engine", "ShowFPS", false);
        WriteString("Engine", "Language", "en");

        _isDirty = false; // Defaults don't need to be saved immediately
    }

    /// <summary>
    /// Gets the full section path with base path prepended.
    /// </summary>
    private string GetFullSection(string section)
    {
        if (string.IsNullOrWhiteSpace(section))
            return BasePath;

        return string.IsNullOrWhiteSpace(BasePath)
            ? section
            : $"{BasePath}/{section}";
    }

    /// <summary>
    /// Throws if disposed.
    /// </summary>
    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(WmeSettings));
    }

    /// <summary>
    /// Disposes the settings manager.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        // Auto-save on dispose if dirty
        if (_isDirty)
        {
            try
            {
                Save();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to auto-save settings on dispose");
            }
        }

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
