using SmartMix.Core.Domain.Enums;
using SmartMix.Core.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace SmartMix.Core.Domain.Entities;

/// <summary>
/// Компонент в составе рецепта
/// </summary>
[DataContract]
public class RecipeComponent : ICloneable<RecipeComponent>
{
    [DataMember]
    [Required]
    public MaterialId ComponentId { get; set; }

    [DataMember]
    [Required, MaxLength(100)]
    public string ComponentName { get; set; } = string.Empty;

    [DataMember]
    [Range(0.01, 100)]
    public decimal Percentage { get; set; }

    [DataMember]
    public MaterialWeight TargetWeight { get; set; }

    [DataMember]
    public decimal Correct { get; set; }

    public RecipeComponent Clone()
    {
        return (RecipeComponent)MemberwiseClone();
    }
}

/// <summary>
/// Настройки времени для рецепта
/// </summary>
[DataContract]
public class RecipeTimeSettings : ICloneable<RecipeTimeSettings>
{
    [DataMember]
    public int Id { get; set; }

    [DataMember]
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [DataMember]
    public TimeSpan MixTime { get; set; } = TimeSpan.FromSeconds(60);

    [DataMember]
    public TimeSpan UnloadTime { get; set; } = TimeSpan.FromSeconds(10);

    [DataMember]
    public TimeSpan ExtraUnloadTime { get; set; } = TimeSpan.FromSeconds(3);

    public RecipeTimeSettings Clone() => (RecipeTimeSettings)MemberwiseClone();
}

/// <summary>
/// Настройки затворов смесителя для рецепта
/// </summary>
[DataContract]
public class RecipeMixerGateSettings : ICloneable<RecipeMixerGateSettings>
{
    [DataMember]
    public int Id { get; set; }

    [DataMember]
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [DataMember]
    public List<MixerGateSetting> Gates { get; set; } = new();

    public RecipeMixerGateSettings Clone()
    {
        var clone = (RecipeMixerGateSettings)MemberwiseClone();
        clone.Gates = Gates.Select(g => g.Clone()).ToList();
        return clone;
    }
}

[DataContract]
public class MixerGateSetting : ICloneable<MixerGateSetting>
{
    [DataMember]
    public int GateNumber { get; set; }

    [DataMember]
    public int PulseCount { get; set; } = 1;

    [DataMember]
    public TimeSpan PulseTime { get; set; } = TimeSpan.FromMilliseconds(500);

    [DataMember]
    public TimeSpan DelayTime { get; set; } = TimeSpan.FromMilliseconds(200);

    [DataMember]
    public bool DontCloseAfterImpulse { get; set; }

    public MixerGateSetting Clone() => (MixerGateSetting)MemberwiseClone();
}

/// <summary>
/// Категория рецепта
/// </summary>
[DataContract]
public class RecipeCategory : Entity<int>, ICloneable<RecipeCategory>
{
    [DataMember]
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [DataMember]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    public RecipeCategory Clone()
    {
        var clone = (RecipeCategory)MemberwiseClone();
        return clone;
    }
}