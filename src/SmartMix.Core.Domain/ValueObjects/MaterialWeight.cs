namespace SmartMix.Core.Domain.ValueObjects;

/// <summary>
/// Вес материала в килограммах (Value Object)
/// </summary>
public readonly record struct MaterialWeight
{
    public decimal Kilograms { get; init; }

    private MaterialWeight(decimal kilograms)
    {
        if (kilograms < 0)
            throw new ArgumentException("Вес не может быть отрицательным", nameof(kilograms));
        Kilograms = Math.Round(kilograms, 3);
    }

    public static MaterialWeight Zero => new(0);
    public static MaterialWeight FromKg(decimal kg) => new(Math.Round(kg, 3));
    public static MaterialWeight FromGrams(int grams) => new(Math.Round(grams / 1000m, 3));
    public static MaterialWeight FromTons(decimal tons) => new(Math.Round(tons * 1000m, 3));

    public int Grams => (int)(Kilograms * 1000);
    public decimal Tons => Math.Round(Kilograms / 1000m, 6);

    public static implicit operator decimal(MaterialWeight weight) => weight.Kilograms;
    public static explicit operator MaterialWeight(decimal kg) => FromKg(kg);

    public static MaterialWeight operator +(MaterialWeight a, MaterialWeight b) => FromKg(a.Kilograms + b.Kilograms);
    public static MaterialWeight operator -(MaterialWeight a, MaterialWeight b) => FromKg(a.Kilograms - b.Kilograms);
    public static MaterialWeight operator *(MaterialWeight a, decimal factor) => FromKg(a.Kilograms * factor);
    public static MaterialWeight operator /(MaterialWeight a, decimal divisor) => FromKg(a.Kilograms / divisor);

    public bool IsZero => Kilograms == 0;
    public bool IsPositive => Kilograms > 0;

    public override string ToString() => $"{Kilograms:F3} кг";
}