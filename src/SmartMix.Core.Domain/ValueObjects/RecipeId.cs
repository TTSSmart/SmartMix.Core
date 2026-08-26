namespace SmartMix.Core.Domain.ValueObjects;

/// <summary>
/// Идентификатор рецепта
/// </summary>
public readonly record struct RecipeId
{
    public int Value { get; init; }

    public static RecipeId Empty => new() { Value = 0 };
    public static implicit operator int(RecipeId id) => id.Value;
    public static explicit operator RecipeId(int value) => new() { Value = value };

    public bool IsEmpty => Value == 0;
    public override string ToString() => Value.ToString();
}