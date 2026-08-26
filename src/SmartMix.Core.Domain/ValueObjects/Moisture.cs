namespace SmartMix.Core.Domain.ValueObjects;

/// <summary>
/// Влажность материала в процентах (Value Object)
/// </summary>
public readonly record struct Moisture
{
    public decimal Percent { get; init; }

    private Moisture(decimal percent)
    {
        Percent = Math.Clamp(Math.Round(percent, 2), 0, 100);
    }

    public static Moisture Zero => new(0);
    public static Moisture FromPercent(decimal percent) => new(percent);
    public static Moisture FromRaw(ushort raw, decimal discrete = 0.1m) => new(raw * discrete);

    public decimal Fraction => Percent / 100m;

    public static implicit operator decimal(Moisture moisture) => moisture.Percent;
    public static explicit operator Moisture(decimal percent) => FromPercent(percent);

    public static Moisture operator +(Moisture a, Moisture b) => FromPercent(a.Percent + b.Percent);
    public static Moisture operator -(Moisture a, Moisture b) => FromPercent(a.Percent - b.Percent);

    public bool IsZero => Percent == 0;
    public bool IsValid => Percent >= 0 && Percent <= 100;

    public override string ToString() => $"{Percent:F2}%";
}