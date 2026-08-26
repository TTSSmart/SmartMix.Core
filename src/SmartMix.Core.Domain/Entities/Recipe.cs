using SmartMix.Core.Domain.Enums;
using SmartMix.Core.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace SmartMix.Core.Domain.Entities;

/// <summary>
/// Рецепт
/// </summary>
[DataContract]
public class Recipe : Entity<int>, ICloneable<Recipe>
{
    [DataMember]
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [DataMember]
    public bool Consider { get; set; } = true;

    [DataMember]
    public bool UseAutoCorrection { get; set; } = true;

    [DataMember]
    public int UserId { get; set; }

    [DataMember]
    public DateTime EditDate { get; set; } = DateTime.UtcNow;

    [DataMember]
    public RecipeId TimeSetId { get; set; } = RecipeId.Empty;

    [DataMember]
    public RecipeTimeSettings? RecipeTimeSettings { get; set; }

    [DataMember]
    public Dictionary<LevelHumidity, RecipeHumidity> CalibLevelHumidity { get; set; } = new()
    {
        { LevelHumidity.Min, new RecipeHumidity() },
        { LevelHumidity.Middle, new RecipeHumidity() },
        { LevelHumidity.Max, new RecipeHumidity() }
    };

    [DataMember]
    public Guid _1CGuid { get; set; } = Guid.Empty;

    [DataMember]
    [MaxLength(50)]
    public string _1CNumber { get; set; } = string.Empty;

    [DataMember]
    public RecipeId MixerSetId { get; set; } = RecipeId.Empty;

    [DataMember]
    public RecipeMixerGateSettings? RecipeMixerSettings { get; set; }

    [DataMember]
    public RecipeCategory? RecipeCategory { get; set; }

    [DataMember]
    public List<RecipeComponent> Components { get; set; } = new();

    [DataMember]
    public bool UseCommonMixTime { get; set; }

    [DataMember]
    public TimeSpan MixTime { get; set; } = TimeSpan.FromSeconds(60);

    // 1C integration
    public string ExternalId => _1CGuid != Guid.Empty ? _1CGuid.ToString() : _1CNumber;

    public decimal TotalPercentage => Components.Sum(c => c.Percentage);
    public bool IsValidForProduction => TotalPercentage > 99.9m && TotalPercentage < 100.1m && Components.All(c => !c.ComponentId.IsEmpty);

    public Recipe Clone()
    {
        var clone = (Recipe)MemberwiseClone();
        clone.Components = Components.Select(c => c.Clone()).ToList();
        clone.RecipeTimeSettings = RecipeTimeSettings?.Clone();
        clone.RecipeMixerSettings = RecipeMixerSettings?.Clone();
        clone.RecipeCategory = RecipeCategory?.Clone();
        clone.CalibLevelHumidity = CalibLevelHumidity.ToDictionary(k => k.Key, v => v.Value.Clone());
        return clone;
    }
}

/// <summary>
/// Калибровка влажности для рецепта
/// </summary>
[DataContract]
public class RecipeHumidity : ICloneable<RecipeHumidity>
{
    [DataMember]
    public Moisture Moisture { get; set; } = Moisture.Zero;

    [DataMember]
    public MaterialWeight Correction { get; set; } = MaterialWeight.Zero;

    public RecipeHumidity Clone() => (RecipeHumidity)MemberwiseClone();
}