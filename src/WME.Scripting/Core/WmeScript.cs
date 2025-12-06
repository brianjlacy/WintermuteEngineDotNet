namespace WME.Scripting.Core;

using WME.Core.Scripting;
using WME.Core.Objects;

/// <summary>
/// Represents a compiled script ready for execution.
/// </summary>
public class WmeScript : IDisposable
{
    private readonly List<WmeInstruction> _instructions = new();
    private readonly Dictionary<string, int> _functions = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, WmeValue> _globals = new(StringComparer.OrdinalIgnoreCase);
    private bool _disposed;

    /// <summary>
    /// Gets the script filename.
    /// </summary>
    public string? Filename { get; set; }

    /// <summary>
    /// Gets the current execution state.
    /// </summary>
    public ScriptState State { get; internal set; } = ScriptState.Ready;

    /// <summary>
    /// Gets the instruction list.
    /// </summary>
    public IReadOnlyList<WmeInstruction> Instructions => _instructions;

    /// <summary>
    /// Gets the function table.
    /// </summary>
    public IReadOnlyDictionary<string, int> Functions => _functions;

    /// <summary>
    /// Gets or sets the 'this' object for script execution.
    /// </summary>
    public WmeObject? ThisObject { get; set; }

    /// <summary>
    /// Gets the instruction pointer (current execution position).
    /// </summary>
    public int InstructionPointer { get; internal set; }

    /// <summary>
    /// Gets whether the script is currently running.
    /// </summary>
    public bool IsRunning => State == ScriptState.Running;

    /// <summary>
    /// Adds an instruction to the script.
    /// </summary>
    internal void AddInstruction(WmeInstruction instruction)
    {
        _instructions.Add(instruction);
    }

    /// <summary>
    /// Registers a function entry point.
    /// </summary>
    internal void RegisterFunction(string name, int address)
    {
        _functions[name] = address;
    }

    /// <summary>
    /// Sets a global variable.
    /// </summary>
    public void SetGlobal(string name, WmeValue value)
    {
        _globals[name] = value;
    }

    /// <summary>
    /// Gets a global variable.
    /// </summary>
    public WmeValue? GetGlobal(string name)
    {
        return _globals.TryGetValue(name, out var value) ? value : null;
    }

    /// <summary>
    /// Resets the script for re-execution.
    /// </summary>
    public void Reset()
    {
        InstructionPointer = 0;
        State = ScriptState.Ready;
    }

    /// <summary>
    /// Disposes the script.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        _instructions.Clear();
        _functions.Clear();
        _globals.Clear();

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Represents the execution state of a script.
/// </summary>
public enum ScriptState
{
    /// <summary>
    /// Script is ready to run.
    /// </summary>
    Ready,

    /// <summary>
    /// Script is currently executing.
    /// </summary>
    Running,

    /// <summary>
    /// Script execution is paused/suspended.
    /// </summary>
    Suspended,

    /// <summary>
    /// Script execution has finished.
    /// </summary>
    Finished,

    /// <summary>
    /// Script execution failed with an error.
    /// </summary>
    Error
}
