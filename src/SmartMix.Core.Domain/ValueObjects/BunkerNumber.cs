namespace SmartMix.Core.Domain.ValueObjects;

/// <summary>
/// Номер бункера (1-64)
/// </summary>
public readonly record struct BunkerNumber
{
    public byte Value { get; init; }

    public static BunkerNumber Empty => new() { Value = 0 };
    public static BunkerNumber From(byte value)
    {
        if (value == 0 || value > 64)
            throw new ArgumentOutOfRangeException(nameof(value), "Bunker number must be 1-64");
        return new() { Value = value };
    }

    public static implicit operator byte(BunkerNumber num) => num.Value;
    public static explicit operator BunkerNumber(byte value) => From(value);
    public static implicit operator int(BunkerNumber num) => num.Value;

    public bool IsEmpty => Value == 0;
    public override string ToString() => Value.ToString();
}