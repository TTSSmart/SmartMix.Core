namespace SmartMix.Core.Domain.ValueObjects;

/// <summary>
/// Идентификатор компонента/материала
/// </summary>
public readonly record struct MaterialId
{
    public int Value { get; init; }

    public static MaterialId Empty => new() { Value = 0 };
    public static implicit operator int(MaterialId id) => id.Value;
    public static explicit operator MaterialId(int value) => new() { Value = value };

    public bool IsEmpty => Value == 0;
    public override string ToString() => Value.ToString();
}