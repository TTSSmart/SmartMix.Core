namespace SmartMix.Core.Application.Common;

/// <summary>
/// Базовый результат операции
/// </summary>
public record Result
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    
    public static Result Ok() => new() { Success = true };
    public static Result Failure(string error) => new() { Success = false, ErrorMessage = error };
}

/// <summary>
/// Результат с данными
/// </summary>
public record Result<T> : Result
{
    public T? Data { get; init; }
    
    public static Result<T> Ok(T data) => new() { Success = true, Data = data };
    public static Result<T> Failure(string error) => new() { Success = false, ErrorMessage = error };
}

/// <summary>
/// Пагинированный результат
/// </summary>
public record PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
    
    public static PagedResult<T> Create(IReadOnlyList<T> items, int totalCount, int page, int pageSize)
        => new() { Items = items, TotalCount = totalCount, Page = page, PageSize = pageSize };
}

/// <summary>
/// Исключение для бизнес-ошибок
/// </summary>
public class BusinessRuleException : Exception
{
    public string ErrorCode { get; }
    
    public BusinessRuleException(string message, string errorCode = "BUSINESS_RULE_VIOLATION") 
        : base(message)
    {
        ErrorCode = errorCode;
    }
}

/// <summary>
/// Исключение для не найденных сущностей
/// </summary>
public class NotFoundException : Exception
{
    public string EntityType { get; }
    public object EntityId { get; }
    
    public NotFoundException(string entityType, object entityId)
        : base($"{entityType} with id {entityId} not found")
    {
        EntityType = entityType;
        EntityId = entityId;
    }
}
