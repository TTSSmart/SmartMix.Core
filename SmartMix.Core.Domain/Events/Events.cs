namespace SmartMix.Core.Domain.Events;
public interface IDomainEventPublisher { void Publish<TEvent>(TEvent domainEvent); }
public sealed class InMemoryDomainEventPublisher : IDomainEventPublisher { public List<object> Events { get; } = []; public void Publish<TEvent>(TEvent domainEvent) => Events.Add(domainEvent!); }
public class BatchStartedEvent { public int ApplicationId { get; init; } public int MixerNumber { get; init; } }
public class BatchCompletedEvent { public int ApplicationId { get; init; } public int MixerNumber { get; init; } public bool Success { get; init; } }
public class MaterialConsumedEvent { public int BunkerNumber { get; init; } public int MaterialId { get; init; } public ValueObjects.MaterialWeight Weight { get; init; } }
public class ConsumptionCountersResetEvent { }
public class RecipeCreatedEvent { public int RecipeId { get; init; } public string RecipeName { get; init; } = string.Empty; public int CreatedBy { get; init; } }
public class RecipeUpdatedEvent { public int RecipeId { get; init; } public string RecipeName { get; init; } = string.Empty; public int UpdatedBy { get; init; } public Dictionary<string, object> Changes { get; init; } = []; }
public class BunkerComponentChangedEvent { public int BunkerNumber { get; init; } public int OldMaterialId { get; init; } public int NewMaterialId { get; init; } public int ChangedBy { get; init; } }
public class BunkerStateChangedEvent { public int BunkerNumber { get; init; } public bool IsActive { get; init; } public int ChangedBy { get; init; } }
public class UserLoggedInEvent { public int UserId { get; init; } public string Username { get; init; } = string.Empty; public string? IpAddress { get; init; } }
public class PlcConnectionLostEvent { public string PlcAddress { get; init; } = string.Empty; public int LineNumber { get; init; } }
public class PlcConnectionRestoredEvent : PlcConnectionLostEvent { }
public class PlcRegistersUpdatedEvent { public string PlcAddress { get; init; } = string.Empty; public int RegisterCount { get; init; } }
