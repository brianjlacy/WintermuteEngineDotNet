namespace WME.Scripting.Core;

/// <summary>
/// Bytecode operation codes for the WME virtual machine.
/// </summary>
public enum OpCode : byte
{
    // Stack operations
    Nop = 0x00,
    PushInt = 0x01,
    PushFloat = 0x02,
    PushString = 0x03,
    PushBool = 0x04,
    PushNull = 0x05,
    PushVar = 0x06,
    PopVar = 0x07,
    PopEmpty = 0x08,

    // Arithmetic operations
    Add = 0x10,
    Sub = 0x11,
    Mul = 0x12,
    Div = 0x13,
    Mod = 0x14,
    Neg = 0x15,

    // Comparison operations
    Equal = 0x20,
    NotEqual = 0x21,
    Less = 0x22,
    Greater = 0x23,
    LessEqual = 0x24,
    GreaterEqual = 0x25,

    // Logical operations
    And = 0x30,
    Or = 0x31,
    Not = 0x32,

    // Control flow
    Jump = 0x40,
    JumpIfFalse = 0x41,
    JumpIfTrue = 0x42,
    Call = 0x43,
    CallMethod = 0x44,
    Return = 0x45,
    ReturnEvent = 0x46,

    // Object operations
    GetProperty = 0x50,
    SetProperty = 0x51,
    CreateObject = 0x52,
    GetMember = 0x53,
    SetMember = 0x54,

    // Array operations
    CreateArray = 0x60,
    GetElement = 0x61,
    SetElement = 0x62,

    // Special operations
    Sleep = 0x70,
    Yield = 0x71,
    Debug = 0x72
}

/// <summary>
/// Represents a single bytecode instruction.
/// </summary>
public struct WmeInstruction
{
    /// <summary>
    /// Gets or sets the operation code.
    /// </summary>
    public OpCode OpCode { get; set; }

    /// <summary>
    /// Gets or sets the first operand.
    /// </summary>
    public object? Operand { get; set; }

    /// <summary>
    /// Gets or sets the second operand (optional).
    /// </summary>
    public object? Operand2 { get; set; }

    /// <summary>
    /// Initializes a new instruction.
    /// </summary>
    public WmeInstruction(OpCode opCode, object? operand = null, object? operand2 = null)
    {
        OpCode = opCode;
        Operand = operand;
        Operand2 = operand2;
    }

    /// <summary>
    /// Returns a string representation of this instruction.
    /// </summary>
    public override readonly string ToString()
    {
        if (Operand != null && Operand2 != null)
            return $"{OpCode} {Operand} {Operand2}";
        if (Operand != null)
            return $"{OpCode} {Operand}";
        return OpCode.ToString();
    }
}
