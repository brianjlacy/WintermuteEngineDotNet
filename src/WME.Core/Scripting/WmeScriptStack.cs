namespace WME.Core.Scripting;

/// <summary>
/// Stack for script execution, managing values and call frames.
/// </summary>
public class WmeScriptStack
{
    private readonly Stack<WmeValue> _stack = new();
    private readonly Stack<CallFrame> _callFrames = new();
    private const int MaxStackSize = 10000;

    /// <summary>
    /// Gets the current stack depth.
    /// </summary>
    public int Depth => _stack.Count;

    /// <summary>
    /// Gets the current call frame depth.
    /// </summary>
    public int CallDepth => _callFrames.Count;

    /// <summary>
    /// Pushes a value onto the stack.
    /// </summary>
    /// <param name="value">The value to push.</param>
    public void Push(WmeValue value)
    {
        if (_stack.Count >= MaxStackSize)
        {
            throw new InvalidOperationException("Stack overflow");
        }

        _stack.Push(value ?? throw new ArgumentNullException(nameof(value)));
    }

    /// <summary>
    /// Pops a value from the stack.
    /// </summary>
    /// <returns>The popped value.</returns>
    public WmeValue Pop()
    {
        if (_stack.Count == 0)
        {
            throw new InvalidOperationException("Stack underflow");
        }

        return _stack.Pop();
    }

    /// <summary>
    /// Peeks at the top value without removing it.
    /// </summary>
    /// <returns>The top value.</returns>
    public WmeValue Peek()
    {
        if (_stack.Count == 0)
        {
            throw new InvalidOperationException("Stack is empty");
        }

        return _stack.Peek();
    }

    /// <summary>
    /// Tries to pop a value from the stack.
    /// </summary>
    /// <param name="value">The popped value, or null if stack is empty.</param>
    /// <returns>True if a value was popped, false if stack is empty.</returns>
    public bool TryPop(out WmeValue? value)
    {
        if (_stack.Count == 0)
        {
            value = null;
            return false;
        }

        value = _stack.Pop();
        return true;
    }

    /// <summary>
    /// Pops a boolean value from the stack.
    /// </summary>
    /// <returns>The boolean value.</returns>
    public bool PopBool()
    {
        var value = Pop();
        return value.Value is bool b ? b : Convert.ToBoolean(value.Value);
    }

    /// <summary>
    /// Pops an integer value from the stack.
    /// </summary>
    /// <returns>The integer value.</returns>
    public int PopInt()
    {
        var value = Pop();
        return Convert.ToInt32(value.Value);
    }

    /// <summary>
    /// Pops a floating-point value from the stack.
    /// </summary>
    /// <returns>The floating-point value.</returns>
    public double PopDouble()
    {
        var value = Pop();
        return Convert.ToDouble(value.Value);
    }

    /// <summary>
    /// Pops a string value from the stack.
    /// </summary>
    /// <returns>The string value.</returns>
    public string? PopString()
    {
        var value = Pop();
        return value.Value?.ToString();
    }

    /// <summary>
    /// Pushes a new call frame onto the call stack.
    /// </summary>
    /// <param name="functionName">Name of the function being called.</param>
    /// <param name="returnAddress">Return address (instruction pointer).</param>
    /// <param name="localCount">Number of local variables.</param>
    public void PushCallFrame(string functionName, int returnAddress, int localCount)
    {
        var frame = new CallFrame
        {
            FunctionName = functionName,
            ReturnAddress = returnAddress,
            StackBase = _stack.Count,
            LocalCount = localCount
        };

        _callFrames.Push(frame);
    }

    /// <summary>
    /// Pops a call frame from the call stack.
    /// </summary>
    /// <returns>The popped call frame.</returns>
    public CallFrame PopCallFrame()
    {
        if (_callFrames.Count == 0)
        {
            throw new InvalidOperationException("No call frames to pop");
        }

        return _callFrames.Pop();
    }

    /// <summary>
    /// Gets the current call frame without removing it.
    /// </summary>
    /// <returns>The current call frame, or null if no frames exist.</returns>
    public CallFrame? GetCurrentFrame()
    {
        return _callFrames.Count > 0 ? _callFrames.Peek() : null;
    }

    /// <summary>
    /// Clears the entire stack.
    /// </summary>
    public void Clear()
    {
        _stack.Clear();
        _callFrames.Clear();
    }

    /// <summary>
    /// Gets the call stack trace for debugging.
    /// </summary>
    /// <returns>String representation of the call stack.</returns>
    public string GetStackTrace()
    {
        var frames = _callFrames.ToArray();
        return string.Join("\n", frames.Select((f, i) =>
            $"  at {f.FunctionName} (frame {i}, locals: {f.LocalCount})"));
    }

    /// <summary>
    /// Represents a function call frame.
    /// </summary>
    public class CallFrame
    {
        /// <summary>
        /// Gets or sets the function name.
        /// </summary>
        public string FunctionName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the return address (instruction pointer).
        /// </summary>
        public int ReturnAddress { get; set; }

        /// <summary>
        /// Gets or sets the base of the stack for this frame.
        /// </summary>
        public int StackBase { get; set; }

        /// <summary>
        /// Gets or sets the number of local variables.
        /// </summary>
        public int LocalCount { get; set; }
    }
}
