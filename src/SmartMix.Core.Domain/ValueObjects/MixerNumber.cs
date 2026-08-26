namespace SmartMix.Core.Domain.ValueObjects;

/// <summary>
/// Номер смесителя (1-8)
/// </summary>
public readonly record struct MixerNumber
{
    public byte Value { get; init; }

    public static MixerNumber Empty => new() { Value = 0 };
    public static MixerNumber From(byte value)
    {
        if (value == 0 || value > 8)
            throw new ArgumentOutOfRangeException(nameof(value), "Mixer number must be 1-8");
        return new() { Value = value };
    }

    public static implicit operator byte(MixerNumber num) => num.Value;
    public static explicit operator MixerNumber(byte value) => From(value);
    public static implicit operator int(MixerNumber num) => num.Value;

    public bool IsEmpty => Value == 0;
    public override string ToString() => Value.ToString();
}