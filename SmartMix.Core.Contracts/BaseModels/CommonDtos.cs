namespace SmartMix.Core.Contracts.BaseModels;

/// <summary>
/// Базовый DTO для ответа API
/// </summary>
public record ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public string? ErrorMessage { get; init; }
    public string? ErrorCode { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    
    public static ApiResponse<T> Ok(T data) => new() { Success = true, Data = data };
    public static ApiResponse<T> Fail(string errorMessage, string? errorCode = null) => new() { Success = false, ErrorMessage = errorMessage, ErrorCode = errorCode };
}

/// <summary>
/// Пагинированный ответ
/// </summary>
public record PagedResponse<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}

/// <summary>
/// Параметры пагинации
/// </summary>
public record PaginationParams
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    
    public int Skip => (Page - 1) * PageSize;
    public int Take => PageSize;
}

/// <summary>
/// Параметры сортировки
/// </summary>
public record SortParams
{
    public string? SortBy { get; init; }
    public bool Descending { get; init; } = false;
}

/// <summary>
/// Фильтр по датам
/// </summary>
public record DateRangeFilter
{
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }
}

/// <summary>
/// Идентификатор сущности
/// </summary>
public record EntityId(int Value)
{
    public static implicit operator int(EntityId id) => id.Value;
    public static explicit operator EntityId(int value) => new(value);
}

/// <summary>
/// Версия сущности для оптимистичной блокировки
/// </summary>
public record EntityVersion(long Value)
{
    public static implicit operator long(EntityVersion v) => v.Value;
    public static explicit operator EntityVersion(long value) => new(value);
}