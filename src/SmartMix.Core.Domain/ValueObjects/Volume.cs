namespace SmartMix.Core.Domain.ValueObjects;

/// <summary>
/// Объем в кубических метрах (Value Object)
/// </summary>
public readonly record struct Volume
{
    public decimal CubicMeters { get; init; }

    private Volume(decimal cubicMeters)
    {
        if (cubicMeters < 0)
            throw new ArgumentException("Объем не может быть отрицательным", nameof(cubicMeters));
        CubicMeters = Math.Round(cubicMeters, 3);
    }

    public static Volume Zero => new(0);
    public static Volume FromCubicMeters(decimal m3) => new(m3);
    public static Volume FromLiters(decimal liters) => new(Math.Round(liters / 1000m, 3));

    public decimal Liters => CubicMeters * 1000m;

    public static implicit operator decimal(Volume vol) => vol.CubicMeters;
    public static explicit operator Volume(decimal m3) => FromCubicMeters(m3);

    public static Volume operator +(Volume a, Volume b) => FromCubicMeters(a.CubicMeters + b.CubicMeters);
    public static Volume operator -(Volume a, Volume b) => FromCubicMeters(a.CubicMeters - b.CubicMeters);
    public static Volume operator *(Volume a, decimal factor) => FromCubicMeters(a.CubicMeters * factor);

    public bool IsZero => CubicMeters == 0;
    public bool IsPositive => CubicMeters > 0;

    public override string ToString() => $"{CubicMeters:F3} м³";
}