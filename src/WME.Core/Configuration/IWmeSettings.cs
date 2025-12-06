namespace WME.Core.Configuration;

/// <summary>
/// Interface for engine and game settings management.
/// Provides cross-platform configuration storage (replaces CBRegistry from original WME).
/// </summary>
public interface IWmeSettings : IDisposable
{
    /// <summary>
    /// Gets or sets the configuration file path.
    /// </summary>
    string ConfigFilePath { get; set; }

    /// <summary>
    /// Gets or sets the base section/category for settings.
    /// </summary>
    string BasePath { get; set; }

    /// <summary>
    /// Reads a string value from settings.
    /// </summary>
    /// <param name="section">Settings section (e.g., "Video", "Audio").</param>
    /// <param name="key">Setting key name.</param>
    /// <param name="defaultValue">Default value if key doesn't exist.</param>
    /// <returns>The setting value or default.</returns>
    string ReadString(string section, string key, string? defaultValue = null);

    /// <summary>
    /// Writes a string value to settings.
    /// </summary>
    /// <param name="section">Settings section.</param>
    /// <param name="key">Setting key name.</param>
    /// <param name="value">Value to write.</param>
    /// <returns>True if successful.</returns>
    bool WriteString(string section, string key, string value);

    /// <summary>
    /// Reads an integer value from settings.
    /// </summary>
    /// <param name="section">Settings section.</param>
    /// <param name="key">Setting key name.</param>
    /// <param name="defaultValue">Default value if key doesn't exist.</param>
    /// <returns>The setting value or default.</returns>
    int ReadInt(string section, string key, int defaultValue = 0);

    /// <summary>
    /// Writes an integer value to settings.
    /// </summary>
    /// <param name="section">Settings section.</param>
    /// <param name="key">Setting key name.</param>
    /// <param name="value">Value to write.</param>
    /// <returns>True if successful.</returns>
    bool WriteInt(string section, string key, int value);

    /// <summary>
    /// Reads a boolean value from settings.
    /// </summary>
    /// <param name="section">Settings section.</param>
    /// <param name="key">Setting key name.</param>
    /// <param name="defaultValue">Default value if key doesn't exist.</param>
    /// <returns>The setting value or default.</returns>
    bool ReadBool(string section, string key, bool defaultValue = false);

    /// <summary>
    /// Writes a boolean value to settings.
    /// </summary>
    /// <param name="section">Settings section.</param>
    /// <param name="key">Setting key name.</param>
    /// <param name="value">Value to write.</param>
    /// <returns>True if successful.</returns>
    bool WriteBool(string section, string key, bool value);

    /// <summary>
    /// Reads a floating-point value from settings.
    /// </summary>
    /// <param name="section">Settings section.</param>
    /// <param name="key">Setting key name.</param>
    /// <param name="defaultValue">Default value if key doesn't exist.</param>
    /// <returns>The setting value or default.</returns>
    float ReadFloat(string section, string key, float defaultValue = 0.0f);

    /// <summary>
    /// Writes a floating-point value to settings.
    /// </summary>
    /// <param name="section">Settings section.</param>
    /// <param name="key">Setting key name.</param>
    /// <param name="value">Value to write.</param>
    /// <returns>True if successful.</returns>
    bool WriteFloat(string section, string key, float value);

    /// <summary>
    /// Checks if a setting key exists.
    /// </summary>
    /// <param name="section">Settings section.</param>
    /// <param name="key">Setting key name.</param>
    /// <returns>True if the key exists.</returns>
    bool HasKey(string section, string key);

    /// <summary>
    /// Removes a setting key.
    /// </summary>
    /// <param name="section">Settings section.</param>
    /// <param name="key">Setting key name.</param>
    /// <returns>True if successful.</returns>
    bool RemoveKey(string section, string key);

    /// <summary>
    /// Loads settings from file.
    /// </summary>
    /// <returns>True if successful.</returns>
    bool Load();

    /// <summary>
    /// Saves settings to file.
    /// </summary>
    /// <returns>True if successful.</returns>
    bool Save();

    /// <summary>
    /// Resets all settings to defaults.
    /// </summary>
    void Clear();
}
