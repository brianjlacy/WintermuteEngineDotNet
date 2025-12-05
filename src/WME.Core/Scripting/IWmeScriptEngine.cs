namespace WME.Core.Scripting;

/// <summary>
/// Script engine interface for compiling and executing WME scripts.
/// </summary>
public interface IWmeScriptEngine : IDisposable
{
    /// <summary>
    /// Loads and compiles a script file.
    /// </summary>
    /// <param name="filename">Path to the script file.</param>
    /// <returns>The compiled script, or null if compilation failed.</returns>
    Task<WmeScript?> LoadScriptAsync(string filename);

    /// <summary>
    /// Compiles script source code to bytecode.
    /// </summary>
    /// <param name="source">The script source code.</param>
    /// <param name="filename">Optional filename for error reporting.</param>
    /// <returns>The compiled script, or null if compilation failed.</returns>
    WmeScript? CompileScript(string source, string? filename = null);

    /// <summary>
    /// Executes a compiled script.
    /// </summary>
    /// <param name="script">The script to execute.</param>
    /// <param name="thisObject">The 'this' object for the script context.</param>
    /// <returns>The script execution result.</returns>
    WmeValue? ExecuteScript(WmeScript script, WmeObject? thisObject = null);

    /// <summary>
    /// Registers a global function that can be called from scripts.
    /// </summary>
    /// <param name="name">The function name as it appears in scripts.</param>
    /// <param name="function">The delegate to invoke.</param>
    void RegisterGlobalFunction(string name, Delegate function);

    /// <summary>
    /// Sets a global variable accessible from all scripts.
    /// </summary>
    /// <param name="name">The variable name.</param>
    /// <param name="value">The variable value.</param>
    void SetGlobalVariable(string name, WmeValue value);

    /// <summary>
    /// Gets a global variable value.
    /// </summary>
    /// <param name="name">The variable name.</param>
    /// <returns>The variable value, or null if not found.</returns>
    WmeValue? GetGlobalVariable(string name);

    /// <summary>
    /// Updates all running scripts. Should be called once per frame.
    /// </summary>
    /// <param name="deltaTime">Time elapsed since last update.</param>
    void Update(TimeSpan deltaTime);
}

/// <summary>
/// Represents a value in the scripting system.
/// </summary>
/// <remarks>
/// Full implementation will be added in Phase 4: Script System.
/// </remarks>
public class WmeValue
{
    /// <summary>
    /// Gets the type of this value.
    /// </summary>
    public WmeValueType Type { get; set; }

    /// <summary>
    /// Gets or sets the underlying value.
    /// </summary>
    public object? Value { get; set; }

    public WmeValue() { }

    public WmeValue(object? value)
    {
        Value = value;
        Type = value switch
        {
            null => WmeValueType.Null,
            bool => WmeValueType.Bool,
            int or long or float or double => WmeValueType.Number,
            string => WmeValueType.String,
            _ => WmeValueType.Object
        };
    }

    public static implicit operator WmeValue(bool value) => new(value);
    public static implicit operator WmeValue(int value) => new(value);
    public static implicit operator WmeValue(float value) => new(value);
    public static implicit operator WmeValue(string value) => new(value);
}

/// <summary>
/// Types of values in the scripting system.
/// </summary>
public enum WmeValueType
{
    Null,
    Bool,
    Number,
    String,
    Object
}
