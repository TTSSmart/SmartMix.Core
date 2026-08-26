namespace SmartMix.Core.Domain.ValueObjects;

/// <summary>
/// Типобезопасный адрес регистра ПЛК
/// </summary>
public readonly record struct RegisterAddress
{
    public ushort Value { get; init; }

    private RegisterAddress(ushort value)
    {
        Value = value;
    }

    public static RegisterAddress From(ushort value) => new() { Value = value };
    public static implicit operator ushort(RegisterAddress address) => address.Value;
    public static implicit operator RegisterAddress(ushort value) => From(value);

    public static RegisterAddress FromNci(string pattern, params object[] args) => From((ushort)int.Parse(string.Format(pattern, args)));
    public static RegisterAddress FromNvo(string pattern, params object[] args) => From((ushort)int.Parse(string.Format(pattern, args)));

    public override string ToString() => $"%MW{Value}";
}