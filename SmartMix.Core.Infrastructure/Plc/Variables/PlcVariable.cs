using SmartMix.Core.Infrastructure.Plc.Enums;

namespace SmartMix.Core.Infrastructure.Plc.Variables;

/// <summary>
/// Базовый класс переменной ПЛК
/// </summary>
public abstract class PlcVariable
{
    public string Name { get; }
    public ushort Address { get; }
    public VariableType Type { get; }
    public VariableAccessLevel AccessLevel { get; }
    public string Description { get; }
    
    protected PlcVariable(string name, ushort address, VariableType type, VariableAccessLevel accessLevel, string description)
    {
        Name = name;
        Address = address;
        Type = type;
        AccessLevel = accessLevel;
        Description = description;
    }
    
    public abstract object GetValue();
    public abstract void UpdateFromRaw(ushort[] rawValues);
    public abstract ushort[] ConvertToRaw(object value);
    public abstract int Size { get; }
}

public sealed class BoolVariable : PlcVariable
{
    public byte BitMask { get; }
    private bool _value;
    
    public BoolVariable(string name, ushort address, byte bitMask, VariableAccessLevel accessLevel, string description)
        : base(name, address, VariableType.Bool, accessLevel, description)
    {
        BitMask = bitMask;
    }
    
    public bool Value => _value;
    
    public override object GetValue() => _value;
    
    public override void UpdateFromRaw(ushort[] rawValues)
    {
        if (rawValues.Length > 0)
            _value = (rawValues[0] & BitMask) != 0;
    }
    
    public override ushort[] ConvertToRaw(object value)
    {
        var boolValue = Convert.ToBoolean(value);
        var current = _value ? (ushort)BitMask : (ushort)0;
        var newValue = boolValue ? (ushort)(current | BitMask) : (ushort)(current & ~BitMask);
        return [(ushort)newValue];
    }
    
    public override int Size => 1;
}

public sealed class IntVariable : PlcVariable
{
    private int _value;
    
    public IntVariable(string name, ushort address, VariableAccessLevel accessLevel, string description)
        : base(name, address, VariableType.Int, accessLevel, description) { }
    
    public int Value => _value;
    
    public override object GetValue() => _value;
    
    public override void UpdateFromRaw(ushort[] rawValues)
    {
        if (rawValues.Length > 0)
            _value = (short)rawValues[0];
    }
    
    public override ushort[] ConvertToRaw(object value)
    {
        var intValue = Convert.ToInt32(value);
        return [(ushort)intValue];
    }
    
    public override int Size => 1;
}

public sealed class UIntVariable : PlcVariable
{
    private uint _value;
    
    public UIntVariable(string name, ushort address, VariableAccessLevel accessLevel, string description)
        : base(name, address, VariableType.Uint, accessLevel, description) { }
    
    public uint Value => _value;
    
    public override object GetValue() => _value;
    
    public override void UpdateFromRaw(ushort[] rawValues)
    {
        if (rawValues.Length > 0)
            _value = rawValues[0];
    }
    
    public override ushort[] ConvertToRaw(object value)
    {
        var uintValue = Convert.ToUInt32(value);
        return [(ushort)uintValue];
    }
    
    public override int Size => 1;
}

public sealed class FloatVariable : PlcVariable
{
    private float _value;
    
    public FloatVariable(string name, ushort address, VariableAccessLevel accessLevel, string description)
        : base(name, address, VariableType.Float, accessLevel, description) { }
    
    public float Value => _value;
    
    public override object GetValue() => _value;
    
    public override void UpdateFromRaw(ushort[] rawValues)
    {
        if (rawValues.Length >= 2)
        {
            var bytes = new byte[4];
            bytes[0] = (byte)(rawValues[0] >> 8);
            bytes[1] = (byte)(rawValues[0] & 0xFF);
            bytes[2] = (byte)(rawValues[1] >> 8);
            bytes[3] = (byte)(rawValues[1] & 0xFF);
            _value = BitConverter.ToSingle(bytes, 0);
        }
    }
    
    public override ushort[] ConvertToRaw(object value)
    {
        var floatValue = Convert.ToSingle(value);
        var bytes = BitConverter.GetBytes(floatValue);
        return [(ushort)(bytes[0] << 8 | bytes[1]), (ushort)(bytes[2] << 8 | bytes[3])];
    }
    
    public override int Size => 2;
}

public sealed class ArrayVariable : PlcVariable
{
    public byte ArraySize { get; }
    private ushort[] _values = Array.Empty<ushort>();
    
    public ArrayVariable(string name, ushort address, byte arraySize, VariableAccessLevel accessLevel, string description)
        : base(name, address, VariableType.Array, accessLevel, description)
    {
        ArraySize = arraySize;
        _values = new ushort[arraySize];
    }
    
    public IReadOnlyList<ushort> Values => _values;
    
    public override object GetValue() => _values.ToArray();
    
    public override void UpdateFromRaw(ushort[] rawValues)
    {
        var length = Math.Min(rawValues.Length, _values.Length);
        Array.Copy(rawValues, _values, length);
    }
    
    public override ushort[] ConvertToRaw(object value)
    {
        if (value is ushort[] arr)
            return arr;
        if (value is IEnumerable<ushort> enumArr)
            return enumArr.ToArray();
        return Array.Empty<ushort>();
    }
    
    public override int Size => ArraySize;
    
    public bool GetBit(int index)
    {
        if (index < 0 || index >= _values.Length * 16) return false;
        var wordIndex = index / 16;
        var bitIndex = index % 16;
        return (_values[wordIndex] & (1 << bitIndex)) != 0;
    }
    
    public void SetBit(int index, bool value)
    {
        if (index < 0 || index >= _values.Length * 16) return;
        var wordIndex = index / 16;
        var bitIndex = index % 16;
        if (value) _values[wordIndex] |= (ushort)(1 << bitIndex);
        else _values[wordIndex] &= (ushort)~(1 << bitIndex);
    }
}