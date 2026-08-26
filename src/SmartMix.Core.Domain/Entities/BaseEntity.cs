using SmartMix.Core.Domain.ValueObjects;
using System.Runtime.Serialization;

namespace SmartMix.Core.Domain.Entities;

/// <summary>
/// Базовый интерфейс для клонирования сущностей
/// </summary>
public interface ICloneable<out T>
{
    T Clone();
}

/// <summary>
/// Базовый класс сущности с идентификатором
/// </summary>
[DataContract]
public abstract class Entity<TId> : ICloneable<Entity<TId>> where TId : struct
{
    [DataMember]
    public TId Id { get; protected set; }

    [DataMember]
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

    [DataMember]
    public DateTime? UpdatedAt { get; protected set; }

    protected Entity() { }

    protected Entity(TId id)
    {
        Id = id;
    }

    public virtual void UpdateTimestamp() => UpdatedAt = DateTime.UtcNow;

    public abstract Entity<TId> Clone();
}

/// <summary>
/// Базовый класс для сущностей с int Id
/// </summary>
[DataContract]
public abstract class Entity : Entity<int>, ICloneable<Entity>
{
    protected Entity() : base() { }
    protected Entity(int id) : base(id) { }

    public override Entity Clone() => (Entity)MemberwiseClone();
}