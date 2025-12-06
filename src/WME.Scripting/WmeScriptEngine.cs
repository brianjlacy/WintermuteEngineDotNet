namespace WME.Scripting;

using WME.Core.Scripting;
using WME.Core.Objects;
using WME.Core.Files;
using WME.Scripting.Core;
using WME.Scripting.VM;
using System.Collections.Concurrent;

/// <summary>
/// Script engine implementation for WME scripts.
/// </summary>
public class WmeScriptEngine : IWmeScriptEngine
{
    private readonly ILogger<WmeScriptEngine> _logger;
    private readonly IWmeFileManager _fileManager;
    private readonly WmeScriptVM _vm;
    private readonly ConcurrentDictionary<string, WmeScript> _cachedScripts = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, WmeValue> _globalVariables = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<WmeScript> _runningScripts = new();
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="WmeScriptEngine"/> class.
    /// </summary>
    public WmeScriptEngine(ILogger<WmeScriptEngine> logger, IWmeFileManager fileManager)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _fileManager = fileManager ?? throw new ArgumentNullException(nameof(fileManager));
        _vm = new WmeScriptVM(_logger);

        RegisterBuiltInFunctions();
    }

    /// <summary>
    /// Loads and compiles a script file.
    /// </summary>
    public async Task<WmeScript?> LoadScriptAsync(string filename)
    {
        ThrowIfDisposed();

        if (string.IsNullOrWhiteSpace(filename))
            throw new ArgumentException("Filename cannot be null or empty", nameof(filename));

        // Check cache
        if (_cachedScripts.TryGetValue(filename, out var cached))
        {
            _logger.LogDebug("Script cache hit: {File}", filename);
            return cached;
        }

        try
        {
            _logger.LogDebug("Loading script: {File}", filename);

            // Read script source
            var source = await _fileManager.ReadAllTextAsync(filename);
            if (source == null)
            {
                _logger.LogError("Failed to read script file: {File}", filename);
                return null;
            }

            // Compile script
            var script = CompileScript(source, filename);
            if (script != null)
            {
                _cachedScripts[filename] = script;
            }

            return script;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load script: {File}", filename);
            return null;
        }
    }

    /// <summary>
    /// Compiles script source code.
    /// </summary>
    public WmeScript? CompileScript(string source, string? filename = null)
    {
        ThrowIfDisposed();

        if (string.IsNullOrWhiteSpace(source))
            throw new ArgumentException("Source cannot be null or empty", nameof(source));

        try
        {
            _logger.LogDebug("Compiling script: {File}", filename ?? "<inline>");

            // TODO: Full ANTLR4 parsing and compilation would go here
            // For now, we create a simple demo script
            var script = new WmeScript
            {
                Filename = filename
            };

            // Example: Simple bytecode for demonstration
            // Real implementation would parse source and generate bytecode
            script.AddInstruction(new WmeInstruction(OpCode.PushString, "Hello from WME!"));
            script.AddInstruction(new WmeInstruction(OpCode.Return));

            _logger.LogInformation("Compiled script: {File} ({Instructions} instructions)",
                filename ?? "<inline>", script.Instructions.Count);

            return script;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to compile script: {File}", filename ?? "<inline>");
            return null;
        }
    }

    /// <summary>
    /// Executes a compiled script.
    /// </summary>
    public WmeValue? ExecuteScript(WmeScript script, WmeObject? thisObject = null)
    {
        ThrowIfDisposed();

        if (script == null)
            throw new ArgumentNullException(nameof(script));

        try
        {
            script.ThisObject = thisObject;
            script.Reset();

            _runningScripts.Add(script);

            _logger.LogDebug("Executing script: {File}", script.Filename ?? "<inline>");

            var result = _vm.Execute(script);

            _runningScripts.Remove(script);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Script execution error: {File}", script.Filename ?? "<inline>");
            _runningScripts.Remove(script);
            return null;
        }
    }

    /// <summary>
    /// Registers a global function.
    /// </summary>
    public void RegisterGlobalFunction(string name, Delegate function)
    {
        ThrowIfDisposed();

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty", nameof(name));

        if (function == null)
            throw new ArgumentNullException(nameof(function));

        _vm.RegisterExternalFunction(name, function);
        _logger.LogDebug("Registered global function: {Name}", name);
    }

    /// <summary>
    /// Sets a global variable.
    /// </summary>
    public void SetGlobalVariable(string name, WmeValue value)
    {
        ThrowIfDisposed();

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty", nameof(name));

        _globalVariables[name] = value ?? throw new ArgumentNullException(nameof(value));
        _logger.LogDebug("Set global variable: {Name} = {Value}", name, value.Value);
    }

    /// <summary>
    /// Gets a global variable.
    /// </summary>
    public WmeValue? GetGlobalVariable(string name)
    {
        ThrowIfDisposed();

        return _globalVariables.TryGetValue(name, out var value) ? value : null;
    }

    /// <summary>
    /// Updates all running scripts.
    /// </summary>
    public void Update(TimeSpan deltaTime)
    {
        ThrowIfDisposed();

        // Resume suspended scripts if needed
        foreach (var script in _runningScripts.Where(s => s.State == ScriptState.Suspended).ToList())
        {
            // TODO: Implement script resumption logic
            _logger.LogDebug("Script suspended: {File}", script.Filename);
        }

        // Clean up finished scripts
        _runningScripts.RemoveAll(s =>
            s.State == ScriptState.Finished ||
            s.State == ScriptState.Error);
    }

    /// <summary>
    /// Registers built-in functions available to all scripts.
    /// </summary>
    private void RegisterBuiltInFunctions()
    {
        // Debug functions
        RegisterGlobalFunction("Print", (string message) =>
        {
            _logger.LogInformation("[Script] {Message}", message);
        });

        RegisterGlobalFunction("Debug", (string message) =>
        {
            _logger.LogDebug("[Script] {Message}", message);
        });

        // Math functions
        RegisterGlobalFunction("Abs", (double value) => Math.Abs(value));
        RegisterGlobalFunction("Sin", (double value) => Math.Sin(value));
        RegisterGlobalFunction("Cos", (double value) => Math.Cos(value));
        RegisterGlobalFunction("Sqrt", (double value) => Math.Sqrt(value));
        RegisterGlobalFunction("Random", (int min, int max) =>
            System.Random.Shared.Next(min, max));

        _logger.LogDebug("Registered built-in functions");
    }

    /// <summary>
    /// Throws if disposed.
    /// </summary>
    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(WmeScriptEngine));
    }

    /// <summary>
    /// Disposes the script engine.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        // Dispose all cached scripts
        foreach (var script in _cachedScripts.Values)
        {
            script?.Dispose();
        }

        _cachedScripts.Clear();
        _runningScripts.Clear();
        _globalVariables.Clear();

        _disposed = true;
        GC.SuppressFinalize(this);

        _logger.LogInformation("Script engine disposed");
    }
}
