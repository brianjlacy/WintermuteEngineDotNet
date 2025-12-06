namespace WME.Scripting.VM;

using WME.Core.Scripting;
using WME.Core.Objects;
using WME.Scripting.Core;

/// <summary>
/// Virtual machine for executing WME script bytecode.
/// </summary>
public class WmeScriptVM
{
    private readonly ILogger _logger;
    private readonly WmeScriptStack _stack;
    private readonly Dictionary<string, Delegate> _externalFunctions = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Initializes a new instance of the <see cref="WmeScriptVM"/> class.
    /// </summary>
    public WmeScriptVM(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _stack = new WmeScriptStack();
    }

    /// <summary>
    /// Registers an external function that can be called from scripts.
    /// </summary>
    public void RegisterExternalFunction(string name, Delegate function)
    {
        _externalFunctions[name] = function ?? throw new ArgumentNullException(nameof(function));
        _logger.LogDebug("Registered external function: {Name}", name);
    }

    /// <summary>
    /// Executes a script.
    /// </summary>
    public WmeValue? Execute(WmeScript script)
    {
        if (script == null)
            throw new ArgumentNullException(nameof(script));

        try
        {
            script.State = ScriptState.Running;
            script.InstructionPointer = 0;
            _stack.Clear();

            while (script.InstructionPointer < script.Instructions.Count &&
                   script.State == ScriptState.Running)
            {
                var instruction = script.Instructions[script.InstructionPointer];
                ExecuteInstruction(script, instruction);

                if (script.State == ScriptState.Running)
                {
                    script.InstructionPointer++;
                }
            }

            if (script.State == ScriptState.Running)
            {
                script.State = ScriptState.Finished;
            }

            // Return top of stack if any
            return _stack.TryPop(out var result) ? result : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Script execution error: {Message}", ex.Message);
            script.State = ScriptState.Error;
            return null;
        }
    }

    /// <summary>
    /// Executes a single instruction.
    /// </summary>
    private void ExecuteInstruction(WmeScript script, WmeInstruction instruction)
    {
        switch (instruction.OpCode)
        {
            // Stack operations
            case OpCode.Nop:
                break;

            case OpCode.PushInt:
                _stack.Push(new WmeValue(Convert.ToInt32(instruction.Operand)));
                break;

            case OpCode.PushFloat:
                _stack.Push(new WmeValue(Convert.ToDouble(instruction.Operand)));
                break;

            case OpCode.PushString:
                _stack.Push(new WmeValue(instruction.Operand?.ToString()));
                break;

            case OpCode.PushBool:
                _stack.Push(new WmeValue(Convert.ToBoolean(instruction.Operand)));
                break;

            case OpCode.PushNull:
                _stack.Push(new WmeValue(null));
                break;

            case OpCode.PushVar:
                var varName = instruction.Operand?.ToString();
                if (varName != null)
                {
                    var value = script.GetGlobal(varName) ?? new WmeValue(null);
                    _stack.Push(value);
                }
                break;

            case OpCode.PopVar:
                var popVarName = instruction.Operand?.ToString();
                if (popVarName != null && _stack.TryPop(out var popValue))
                {
                    script.SetGlobal(popVarName, popValue);
                }
                break;

            case OpCode.PopEmpty:
                _stack.Pop();
                break;

            // Arithmetic operations
            case OpCode.Add:
                ExecuteAdd();
                break;

            case OpCode.Sub:
                ExecuteSub();
                break;

            case OpCode.Mul:
                ExecuteMul();
                break;

            case OpCode.Div:
                ExecuteDiv();
                break;

            case OpCode.Mod:
                ExecuteMod();
                break;

            case OpCode.Neg:
                ExecuteNeg();
                break;

            // Comparison operations
            case OpCode.Equal:
                ExecuteEqual();
                break;

            case OpCode.NotEqual:
                ExecuteNotEqual();
                break;

            case OpCode.Less:
                ExecuteLess();
                break;

            case OpCode.Greater:
                ExecuteGreater();
                break;

            case OpCode.LessEqual:
                ExecuteLessEqual();
                break;

            case OpCode.GreaterEqual:
                ExecuteGreaterEqual();
                break;

            // Logical operations
            case OpCode.And:
                ExecuteAnd();
                break;

            case OpCode.Or:
                ExecuteOr();
                break;

            case OpCode.Not:
                ExecuteNot();
                break;

            // Control flow
            case OpCode.Jump:
                script.InstructionPointer = Convert.ToInt32(instruction.Operand) - 1;
                break;

            case OpCode.JumpIfFalse:
                if (!_stack.PopBool())
                {
                    script.InstructionPointer = Convert.ToInt32(instruction.Operand) - 1;
                }
                break;

            case OpCode.JumpIfTrue:
                if (_stack.PopBool())
                {
                    script.InstructionPointer = Convert.ToInt32(instruction.Operand) - 1;
                }
                break;

            case OpCode.Call:
                ExecuteCall(script, instruction);
                break;

            case OpCode.CallMethod:
                ExecuteCallMethod(instruction);
                break;

            case OpCode.Return:
                script.State = ScriptState.Finished;
                break;

            // Object operations
            case OpCode.GetProperty:
                ExecuteGetProperty(script, instruction);
                break;

            case OpCode.SetProperty:
                ExecuteSetProperty(script, instruction);
                break;

            // Special operations
            case OpCode.Sleep:
                script.State = ScriptState.Suspended;
                break;

            case OpCode.Debug:
                _logger.LogDebug("Debug breakpoint");
                break;

            default:
                _logger.LogWarning("Unknown opcode: {OpCode}", instruction.OpCode);
                break;
        }
    }

    // Arithmetic operation helpers
    private void ExecuteAdd()
    {
        var b = _stack.Pop();
        var a = _stack.Pop();

        // String concatenation
        if (a.Type == WmeValueType.String || b.Type == WmeValueType.String)
        {
            _stack.Push(new WmeValue(a.Value?.ToString() + b.Value?.ToString()));
        }
        // Numeric addition
        else
        {
            var result = Convert.ToDouble(a.Value) + Convert.ToDouble(b.Value);
            _stack.Push(new WmeValue(result));
        }
    }

    private void ExecuteSub()
    {
        var b = _stack.Pop();
        var a = _stack.Pop();
        var result = Convert.ToDouble(a.Value) - Convert.ToDouble(b.Value);
        _stack.Push(new WmeValue(result));
    }

    private void ExecuteMul()
    {
        var b = _stack.Pop();
        var a = _stack.Pop();
        var result = Convert.ToDouble(a.Value) * Convert.ToDouble(b.Value);
        _stack.Push(new WmeValue(result));
    }

    private void ExecuteDiv()
    {
        var b = _stack.Pop();
        var a = _stack.Pop();
        var divisor = Convert.ToDouble(b.Value);
        if (Math.Abs(divisor) < double.Epsilon)
        {
            _logger.LogWarning("Division by zero");
            _stack.Push(new WmeValue(0.0));
        }
        else
        {
            var result = Convert.ToDouble(a.Value) / divisor;
            _stack.Push(new WmeValue(result));
        }
    }

    private void ExecuteMod()
    {
        var b = _stack.Pop();
        var a = _stack.Pop();
        var result = Convert.ToInt32(a.Value) % Convert.ToInt32(b.Value);
        _stack.Push(new WmeValue(result));
    }

    private void ExecuteNeg()
    {
        var a = _stack.Pop();
        var result = -Convert.ToDouble(a.Value);
        _stack.Push(new WmeValue(result));
    }

    // Comparison operation helpers
    private void ExecuteEqual()
    {
        var b = _stack.Pop();
        var a = _stack.Pop();
        var result = Equals(a.Value, b.Value);
        _stack.Push(new WmeValue(result));
    }

    private void ExecuteNotEqual()
    {
        var b = _stack.Pop();
        var a = _stack.Pop();
        var result = !Equals(a.Value, b.Value);
        _stack.Push(new WmeValue(result));
    }

    private void ExecuteLess()
    {
        var b = _stack.Pop();
        var a = _stack.Pop();
        var result = Convert.ToDouble(a.Value) < Convert.ToDouble(b.Value);
        _stack.Push(new WmeValue(result));
    }

    private void ExecuteGreater()
    {
        var b = _stack.Pop();
        var a = _stack.Pop();
        var result = Convert.ToDouble(a.Value) > Convert.ToDouble(b.Value);
        _stack.Push(new WmeValue(result));
    }

    private void ExecuteLessEqual()
    {
        var b = _stack.Pop();
        var a = _stack.Pop();
        var result = Convert.ToDouble(a.Value) <= Convert.ToDouble(b.Value);
        _stack.Push(new WmeValue(result));
    }

    private void ExecuteGreaterEqual()
    {
        var b = _stack.Pop();
        var a = _stack.Pop();
        var result = Convert.ToDouble(a.Value) >= Convert.ToDouble(b.Value);
        _stack.Push(new WmeValue(result));
    }

    // Logical operation helpers
    private void ExecuteAnd()
    {
        var b = _stack.PopBool();
        var a = _stack.PopBool();
        _stack.Push(new WmeValue(a && b));
    }

    private void ExecuteOr()
    {
        var b = _stack.PopBool();
        var a = _stack.PopBool();
        _stack.Push(new WmeValue(a || b));
    }

    private void ExecuteNot()
    {
        var a = _stack.PopBool();
        _stack.Push(new WmeValue(!a));
    }

    // Control flow helpers
    private void ExecuteCall(WmeScript script, WmeInstruction instruction)
    {
        var functionName = instruction.Operand?.ToString();
        if (functionName == null) return;

        // Check for external function
        if (_externalFunctions.TryGetValue(functionName, out var externalFunc))
        {
            // Call external function
            var argCount = Convert.ToInt32(instruction.Operand2 ?? 0);
            var args = new object?[argCount];
            for (int i = argCount - 1; i >= 0; i--)
            {
                args[i] = _stack.Pop().Value;
            }

            var result = externalFunc.DynamicInvoke(args);
            if (result != null)
            {
                _stack.Push(new WmeValue(result));
            }
        }
        // Check for script function
        else if (script.Functions.TryGetValue(functionName, out var address))
        {
            // TODO: Implement function call stack frames
            _logger.LogDebug("Calling script function: {Function}", functionName);
        }
    }

    private void ExecuteCallMethod(WmeInstruction instruction)
    {
        var methodName = instruction.Operand?.ToString();
        if (methodName == null) return;

        var argCount = Convert.ToInt32(instruction.Operand2 ?? 0);
        var args = new WmeValue[argCount];
        for (int i = argCount - 1; i >= 0; i--)
        {
            args[i] = _stack.Pop();
        }

        var obj = _stack.Pop();
        if (obj.Value is WmeScriptable scriptable)
        {
            var result = scriptable.CallMethod(methodName, args);
            if (result != null)
            {
                _stack.Push(result);
            }
        }
    }

    private void ExecuteGetProperty(WmeScript script, WmeInstruction instruction)
    {
        var propertyName = instruction.Operand?.ToString();
        if (propertyName == null) return;

        if (script.ThisObject is WmeScriptable scriptable)
        {
            var value = scriptable.GetProperty(propertyName) ?? new WmeValue(null);
            _stack.Push(value);
        }
    }

    private void ExecuteSetProperty(WmeScript script, WmeInstruction instruction)
    {
        var propertyName = instruction.Operand?.ToString();
        if (propertyName == null) return;

        var value = _stack.Pop();

        if (script.ThisObject is WmeScriptable scriptable)
        {
            scriptable.SetProperty(propertyName, value);
        }
    }
}
