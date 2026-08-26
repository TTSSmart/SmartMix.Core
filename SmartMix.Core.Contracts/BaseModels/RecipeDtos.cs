namespace SmartMix.Core.Contracts.BaseModels;

/// <summary>
/// DTO рецепта
/// </summary>
public record RecipeDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool Consider { get; init; }
    public bool UseAutoCorrection { get; init; }
    public int UserId { get; init; }
    public DateTime EditDate { get; init; }
    public int CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public int TimeSetId { get; init; }
    public int MixerSetId { get; init; }
    public List<RecipeStructureDto> Structures { get; init; } = new();
}

public record RecipeStructureDto
{
    public int ComponentId { get; init; }
    public string ComponentName { get; init; } = string.Empty;
    public decimal Percentage { get; init; }
    public decimal TargetWeightKg { get; init; }
    public decimal Correct { get; init; }
}

/// <summary>
/// DTO для создания рецепта
/// </summary>
public record CreateRecipeRequest
{
    public string Name { get; init; } = string.Empty;
    public bool Consider { get; init; } = true;
    public bool UseAutoCorrection { get; init; } = true;
    public int UserId { get; init; }
    public int CategoryId { get; init; }
    public int TimeSetId { get; init; }
    public int MixerSetId { get; init; }
    public List<CreateRecipeStructureRequest> Structures { get; init; } = new();
}

public record CreateRecipeStructureRequest
{
    public int ComponentId { get; init; }
    public string ComponentName { get; init; } = string.Empty;
    public decimal Percentage { get; init; }
    public decimal TargetWeightKg { get; init; }
    public decimal Correct { get; init; }
}

/// <summary>
/// DTO для обновления рецепта
/// </summary>
public record UpdateRecipeRequest
{
    public string Name { get; init; } = string.Empty;
    public bool Consider { get; init; }
    public bool UseAutoCorrection { get; init; }
    public int UserId { get; init; }
    public List<CreateRecipeStructureRequest>? Structures { get; init; }
}

/// <summary>
/// DTO расчета материалов
/// </summary>
public record CalculateMaterialsRequest
{
    public int RecipeId { get; init; }
    public decimal TotalVolumeM3 { get; init; }
}

public record CalculateMaterialsResponse
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public decimal TotalVolumeM3 { get; init; }
    public List<MaterialVolumeDto> Materials { get; init; } = new();
}

public record MaterialVolumeDto
{
    public int MaterialId { get; init; }
    public string MaterialName { get; init; } = string.Empty;
    public decimal VolumeM3 { get; init; }
    public decimal WeightKg { get; init; }
    public decimal Percentage { get; init; }
    public decimal MoisturePercent { get; init; }
}