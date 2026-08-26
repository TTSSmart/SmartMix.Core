namespace SmartMix.Core.Domain.ValueObjects;

/// <summary>
/// Температура в градусах Цельсия (Value Object)
/// </summary>
public readonly record struct Temperature
{
    public decimal Celsius { get; init; }

    private Temperature(decimal celsius)
    {
        Celsius = Math.Round(celsius, 1);
    }

    public static Temperature Zero => new(0);
    public static Temperature FromCelsius(decimal celsius) => new(celsius);
    public static Temperature FromFahrenheit(decimal fahrenheit) => new(Math.Round((fahrenheit - 32m) * 5m / 9m, 1));
    public static Temperature FromKelvin(decimal kelvin) => new(Math.Round(kelvin - 273.15m, 1));
    public static Temperature FromRaw(ushort raw, decimal discrete = 0.1m) => new(raw * discrete);

    public decimal Fahrenheit => Math.Round(Celsius * 9m / 5m + 32m, 1);
    public decimal Kelvin => Math.Round(Celsius + 273.15m, 2);

    public static implicit operator decimal(Temperature temp) => temp.Celsius;
    public static explicit operator Temperature(decimal celsius) => FromCelsius(celsius);

    public static Temperature operator +(Temperature a, Temperature b) => FromCelsius(a.Celsius + b.Celsius);
    public static Temperature operator -(Temperature a, Temperature b) => FromCelsius(a.Celsius - b.Celsius);

    public override string ToString() => $"{Celsius:F1}°C";
}