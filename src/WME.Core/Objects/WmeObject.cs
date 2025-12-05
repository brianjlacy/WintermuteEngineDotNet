namespace WME.Core.Objects;

/// <summary>
/// Base class for all game objects in Wintermute Engine.
/// Extends <see cref="WmeScriptable"/> with update, render, and event handling capabilities.
/// </summary>
public abstract class WmeObject : WmeScriptable
{
    private bool _visible = true;
    private bool _active = true;
    private int _priority;

    /// <summary>
    /// Gets or sets whether the object is visible and should be rendered.
    /// </summary>
    public bool Visible
    {
        get => _visible;
        set
        {
            if (_visible != value)
            {
                _visible = value;
                OnVisibilityChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets whether the object is active and should receive updates.
    /// </summary>
    public bool Active
    {
        get => _active;
        set
        {
            if (_active != value)
            {
                _active = value;
                OnActiveChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the render priority (higher values render later/on top).
    /// </summary>
    public int Priority
    {
        get => _priority;
        set => _priority = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WmeObject"/> class.
    /// </summary>
    /// <param name="logger">Logger instance for this object.</param>
    protected WmeObject(ILogger logger) : base(logger)
    {
    }

    /// <summary>
    /// Updates the object logic for the current frame.
    /// </summary>
    /// <param name="deltaTime">Time elapsed since last update.</param>
    /// <returns>True if the update was successful.</returns>
    public virtual bool Update(TimeSpan deltaTime)
    {
        ThrowIfDisposed();
        return Active;
    }

    /// <summary>
    /// Renders the object to the current render target.
    /// </summary>
    /// <returns>True if rendering was successful.</returns>
    public virtual bool Render()
    {
        ThrowIfDisposed();
        return Visible;
    }

    /// <summary>
    /// Called when the object's visibility changes.
    /// </summary>
    protected virtual void OnVisibilityChanged()
    {
    }

    /// <summary>
    /// Called when the object's active state changes.
    /// </summary>
    protected virtual void OnActiveChanged()
    {
    }

    /// <summary>
    /// Gets built-in property values for scripting.
    /// </summary>
    protected override WmeValue? GetBuiltInProperty(string name)
    {
        return name switch
        {
            "Visible" => new WmeValue(_visible),
            "Active" => new WmeValue(_active),
            "Priority" => new WmeValue(_priority),
            _ => base.GetBuiltInProperty(name)
        };
    }

    /// <summary>
    /// Sets built-in property values from scripts.
    /// </summary>
    protected override bool SetBuiltInProperty(string name, WmeValue value)
    {
        switch (name)
        {
            case "Visible":
                if (value.Value is bool boolVal)
                {
                    Visible = boolVal;
                    return true;
                }
                break;

            case "Active":
                if (value.Value is bool activeVal)
                {
                    Active = activeVal;
                    return true;
                }
                break;

            case "Priority":
                if (value.Value is int intVal)
                {
                    Priority = intVal;
                    return true;
                }
                break;
        }

        return base.SetBuiltInProperty(name, value);
    }

    /// <summary>
    /// Persists the game object state.
    /// </summary>
    public override bool Persist(IPersistenceManager persistMgr)
    {
        if (!base.Persist(persistMgr)) return false;

        persistMgr.Transfer(nameof(Visible), ref _visible);
        persistMgr.Transfer(nameof(Active), ref _active);
        persistMgr.Transfer(nameof(Priority), ref _priority);

        return true;
    }
}
