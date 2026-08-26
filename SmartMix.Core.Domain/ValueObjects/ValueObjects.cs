namespace SmartMix.Core.Domain.ValueObjects;

public readonly record struct MaterialId(int Value)
{
    public static MaterialId Empty => new(0);
    public static implicit operator int(MaterialId value) => value.Value;
    public static implicit operator MaterialId(int value) => new(value);
}

public static class MaterialIdExtensions
{
    public static MaterialId ToMaterialId(this int value) => new(value);
}

public readonly record struct MaterialWeight(decimal Kilograms)
{
    public decimal Grams => Kilograms * 1000m;
    public decimal Tons => Kilograms / 1000m;
    public bool IsZero => Kilograms == 0m;
    public static MaterialWeight Zero => new(0m);
    public static MaterialWeight FromKg(decimal value) => new(value);
    public static MaterialWeight FromGrams(decimal value) => new(value / 1000m);
    public static MaterialWeight operator +(MaterialWeight left, MaterialWeight right) => new(left.Kilograms + right.Kilograms);
    public static MaterialWeight operator -(MaterialWeight left, MaterialWeight right) => new(left.Kilograms - right.Kilograms);
    public static MaterialWeight operator *(MaterialWeight value, decimal multiplier) => new(value.Kilograms * multiplier);
    public static MaterialWeight operator /(MaterialWeight value, decimal divisor) => new(value.Kilograms / divisor);
    public static implicit operator decimal(MaterialWeight value) => value.Kilograms;
    public static explicit operator MaterialWeight(decimal value) => FromKg(value);
    public override string ToString() => $"{Kilograms:0.###} кг";
}

public readonly record struct Moisture(decimal Percent)
{
    public decimal Fraction => Percent / 100m;
    public bool IsValid => Percent is >= 0m and <= 100m;
    public static Moisture FromPercent(decimal value) => new(Math.Clamp(value, 0m, 100m));
    public static Moisture FromRaw(decimal value, decimal scale) => FromPercent(value * scale);
    public static Moisture operator +(Moisture left, Moisture right) => FromPercent(left.Percent + right.Percent);
}

public readonly record struct Temperature(decimal Celsius)
{
    public decimal Fahrenheit => Celsius * 9m / 5m + 32m;
    public decimal Kelvin => Celsius + 273.15m;
    public static Temperature FromCelsius(decimal value) => new(value);
    public static Temperature FromFahrenheit(decimal value) => FromCelsius((value - 32m) * 5m / 9m);
    public static Temperature FromKelvin(decimal value) => FromCelsius(value - 273.15m);
    public static Temperature operator +(Temperature left, Temperature right) => new(left.Celsius + right.Celsius);
}

public readonly record struct Volume(decimal CubicMeters)
{
    public decimal Liters => CubicMeters * 1000m;
    public static Volume Zero => new(0m);
    public static Volume FromCubicMeters(decimal value) => new(value);
    public static Volume FromLiters(decimal value) => new(value / 1000m);
    public static Volume operator +(Volume left, Volume right) => new(left.CubicMeters + right.CubicMeters);
}

public sealed record PlcEndpoint(string Host, int Port = 502, byte UnitId = 1)
{
    public string ConnectionString => $"{Host}:{Port}";
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Host)) throw new ArgumentException("PLC host must be specified", nameof(Host));
        if (Port is < 1 or > 65535) throw new ArgumentOutOfRangeException(nameof(Port), "Port must be between 1 and 65535");
    }
}
