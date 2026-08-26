using SmartMix.Core.Domain.Enums;
using SmartMix.Core.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace SmartMix.Core.Domain.Entities;

/// <summary>
/// Компонент/Материал
/// </summary>
[DataContract]
public class Component : Entity<int>, ICloneable<Component>
{
    [DataMember]
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [DataMember]
    public ComponentType Type { get; set; }

    [DataMember]
    public MaterialWeight Density { get; set; } = MaterialWeight.Zero;

    [DataMember]
    public Moisture DefaultMoisture { get; set; } = Moisture.Zero;

    [DataMember]
    public bool IsActive { get; set; } = true;

    [DataMember]
    public int SortOrder { get; set; }

    public Component Clone() => (Component)MemberwiseClone();
}

/// <summary>
/// Клиент
/// </summary>
[DataContract]
public class Client : Entity<int>, ICloneable<Client>
{
    [DataMember]
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [DataMember]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [DataMember]
    [MaxLength(20)]
    public string INN { get; set; } = string.Empty;

    [DataMember]
    [MaxLength(255)]
    public string Address { get; set; } = string.Empty;

    [DataMember]
    [MaxLength(50)]
    public string Phone { get; set; } = string.Empty;

    [DataMember]
    public bool IsActive { get; set; } = true;

    public Client Clone() => (Client)MemberwiseClone();
}

/// <summary>
/// Машина (автомобиль)
/// </summary>
[DataContract]
public class Car : Entity<int>, ICloneable<Car>
{
    [DataMember]
    [Required, MaxLength(20)]
    public string Number { get; set; } = string.Empty;

    [DataMember]
    [MaxLength(100)]
    public string Model { get; set; } = string.Empty;

    [DataMember]
    public int ClientId { get; set; }

    [DataMember]
    public MaterialWeight TaraWeight { get; set; } = MaterialWeight.Zero;

    [DataMember]
    public Volume MaxVolume { get; set; } = Volume.Zero;

    [DataMember]
    public bool IsActive { get; set; } = true;

    public Car Clone() => (Car)MemberwiseClone();
}

/// <summary>
/// Продукция
/// </summary>
[DataContract]
public class Product : Entity<int>, ICloneable<Product>
{
    [DataMember]
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [DataMember]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [DataMember]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [DataMember]
    public List<ProductLayer> Layers { get; set; } = new();

    public Product Clone()
    {
        var clone = (Product)MemberwiseClone();
        clone.Layers = Layers.Select(l => l.Clone()).ToList();
        return clone;
    }
}

[DataContract]
public class ProductLayer : ICloneable<ProductLayer>
{
    [DataMember]
    public int Number { get; set; }

    [DataMember]
    public RecipeId RecipeId { get; set; }

    [DataMember]
    public Volume Volume { get; set; }

    public ProductLayer Clone() => (ProductLayer)MemberwiseClone();
}