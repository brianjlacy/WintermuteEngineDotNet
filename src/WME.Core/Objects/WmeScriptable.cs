namespace WME.Core.Objects;

/// <summary>
/// Base class for objects that can be accessed and manipulated from scripts.
/// Extends <see cref="WmeBase"/> with scripting capabilities.
/// </summary>
public abstract class WmeScriptable : WmeBase
{
    private readonly Dictionary<string, WmeValue> _scriptProperties = new();
    private readonly Dictionary<string, Delegate> _scriptMethods = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="WmeScriptable"/> class.
    /// </summary>
    /// <param name="logger">Logger instance for this object.</param>
    protected WmeScriptable(ILogger logger) : base(logger)
    {
    }

    /// <summary>
    /// Gets a property value by name (called from scripts).
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <returns>The property value, or null if not found.</returns>
    public virtual WmeValue? GetProperty(string name)
    {
        ThrowIfDisposed();

        // Check custom properties first
        if (_scriptProperties.TryGetValue(name, out var value))
        {
            return value;
        }

        // Check built-in properties
        return GetBuiltInProperty(name);
    }

    /// <summary>
    /// Sets a property value by name (called from scripts).
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <param name="value">The value to set.</param>
    /// <returns>True if the property was set successfully.</returns>
    public virtual bool SetProperty(string name, WmeValue value)
    {
        ThrowIfDisposed();

        // Try to set built-in property first
        if (SetBuiltInProperty(name, value))
        {
            return true;
        }

        // Store as custom property
        _scriptProperties[name] = value;
        return true;
    }

    /// <summary>
    /// Calls a method by name (called from scripts).
    /// </summary>
    /// <param name="name">The method name.</param>
    /// <param name="args">Method arguments.</param>
    /// <returns>The method return value, or null if method doesn't return a value.</returns>
    public virtual WmeValue? CallMethod(string name, params WmeValue[] args)
    {
        ThrowIfDisposed();

        // Check registered script methods
        if (_scriptMethods.TryGetValue(name, out var method))
        {
            try
            {
                var result = method.DynamicInvoke(args.Select(v => v.Value).ToArray());
                return result != null ? new WmeValue(result) : null;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error calling script method '{Method}' on object '{Object}'", name, ToString());
                return null;
            }
        }

        // Check built-in methods
        return CallBuiltInMethod(name, args);
    }

    /// <summary>
    /// Registers a script method that can be called from scripts.
    /// </summary>
    /// <param name="name">The method name as it appears in scripts.</param>
    /// <param name="method">The delegate to invoke.</param>
    protected void RegisterScriptMethod(string name, Delegate method)
    {
        _scriptMethods[name] = method;
    }

    /// <summary>
    /// Gets a built-in property value. Override in derived classes to expose properties to scripts.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <returns>The property value, or null if not found.</returns>
    protected virtual WmeValue? GetBuiltInProperty(string name)
    {
        return name switch
        {
            "Name" => new WmeValue(Name),
            "Type" => new WmeValue(GetType().Name),
            _ => null
        };
    }

    /// <summary>
    /// Sets a built-in property value. Override in derived classes to expose settable properties to scripts.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <param name="value">The value to set.</param>
    /// <returns>True if the property was set, false if not recognized.</returns>
    protected virtual bool SetBuiltInProperty(string name, WmeValue value)
    {
        switch (name)
        {
            case "Name":
                Name = value.Value?.ToString();
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// Calls a built-in method. Override in derived classes to expose methods to scripts.
    /// </summary>
    /// <param name="name">The method name.</param>
    /// <param name="args">Method arguments.</param>
    /// <returns>The method return value, or null if method not found or doesn't return a value.</returns>
    protected virtual WmeValue? CallBuiltInMethod(string name, WmeValue[] args)
    {
        return null;
    }

    /// <summary>
    /// Persists the scriptable object state.
    /// </summary>
    public override bool Persist(IPersistenceManager persistMgr)
    {
        if (!base.Persist(persistMgr)) return false;

        // Persist custom script properties
        if (persistMgr.IsSaving)
        {
            var propCount = _scriptProperties.Count;
            persistMgr.Transfer("ScriptPropertyCount", ref propCount);

            foreach (var kvp in _scriptProperties)
            {
                var key = kvp.Key;
                var value = kvp.Value;
                persistMgr.Transfer("PropKey", ref key);
                persistMgr.Transfer("PropValue", ref value);
            }
        }
        else
        {
            var propCount = 0;
            persistMgr.Transfer("ScriptPropertyCount", ref propCount);

            _scriptProperties.Clear();
            for (int i = 0; i < propCount; i++)
            {
                string? key = null;
                WmeValue? value = null;
                persistMgr.Transfer("PropKey", ref key);
                persistMgr.Transfer("PropValue", ref value);
                if (key != null && value != null)
                {
                    _scriptProperties[key] = value;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Disposes managed resources including script properties.
    /// </summary>
    protected override void DisposeManagedResources()
    {
        base.DisposeManagedResources();
        _scriptProperties.Clear();
        _scriptMethods.Clear();
    }
}
