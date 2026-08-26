using Microsoft.Extensions.Logging;
using SmartMix.Core.Application.Abstractions;
using SmartMix.Core.Domain.Entities;
using SmartMix.Core.Domain.ValueObjects;
using Dapper;
using MySql.Data.MySqlClient;

namespace SmartMix.Core.Infrastructure.Persistence.Repositories;

public class ApplicationRepository : IApplicationRepository
{
    private readonly ISmartMixDatabase _database;
    private readonly ILogger<ApplicationRepository> _logger;
    
    public ApplicationRepository(ISmartMixDatabase database, ILogger<ApplicationRepository> logger)
    {
        _database = database;
        _logger = logger;
    }
    
    public async Task<Application?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        const string sql = @"SELECT * FROM applications WHERE id = @Id";
        using var conn = _database.GetConnection();
        return await conn.QueryFirstOrDefaultAsync<Application>(sql, new { Id = id });
    }
    
    public async Task<Application?> GetByIdWithLayersAsync(int id, CancellationToken ct = default)
    {
        const string sql = @"
            SELECT a.*, l.* FROM applications a
            LEFT JOIN application_layers l ON a.id = l.application_id
            WHERE a.id = @Id
            ORDER BY l.number";
        
        using var conn = _database.GetConnection();
        var dict = new Dictionary<int, Application>();
        
        var result = await conn.QueryAsync<Application, LayerApplication, Application>(
            sql,
            (app, layer) =>
            {
                if (!dict.TryGetValue(app.Id, out var a))
                {
                    a = app;
                    a.Layers = new List<LayerApplication>();
                    dict[app.Id] = a;
                }
                if (layer != null && layer.Id > 0)
                    a.Layers.Add(layer);
                return a;
            },
            new { Id = id },
            splitOn: "id");
        
        return result.FirstOrDefault();
    }
    
    public async Task<IReadOnlyList<Application>> GetActiveAsync(int lineNumber, CancellationToken ct = default)
    {
        const string sql = @"SELECT * FROM applications 
            WHERE line_number = @LineNumber AND is_completed = 0 AND is_deleted = 0
            ORDER BY order_num";
        using var conn = _database.GetConnection();
        return (await conn.QueryAsync<Application>(sql, new { LineNumber = lineNumber })).ToList();
    }
    
    public async Task<IReadOnlyList<Application>> GetByStatusAsync(ApplicationStatus status, int lineNumber, CancellationToken ct = default)
    {
        const string sql = @"SELECT * FROM applications 
            WHERE line_number = @LineNumber AND batch_phase = @Status
            ORDER BY order_num";
        using var conn = _database.GetConnection();
        return (await conn.QueryAsync<Application>(sql, new { LineNumber = lineNumber, Status = (int)status })).ToList();
    }
    
    public async Task<int> CreateAsync(Application application, CancellationToken ct = default)
    {
        const string sql = @"
            INSERT INTO applications (waybill, client_id, car_id, volume, fact_volume, volume_current, 
                creation_time, start_time, end_time, is_completed, is_deleted, creator_id, user_id, 
                mixer_number, order_num, batch_phase, train_mode, is_speed_app, last_save_batch_num,
                is_running, is_edit_lock, product_id, unloading_point, correct_water_panel, start_auto,
                line_number)
            VALUES (@WayBill, @ClientId, @CarId, @Volume, @FactVolume, @VolumeCurrent,
                @CreationTime, @StartTime, @EndTime, @IsCompleted, @IsDeleted, @CreatorId, @UserId,
                @MixerNumber, @Order, @BatchPhase, @TrainMode, @IsSpeedApp, @LastSaveBatchNum,
                @IsRunning, @IsEditLock, @ProductId, @UnloadingPoint, @CorrectWaterPanel, @StartAuto,
                @LineNumber);
            SELECT LAST_INSERT_ID();";
        
        using var conn = _database.GetConnection();
        return await conn.ExecuteScalarAsync<int>(sql, application);
    }
    
    public async Task UpdateAsync(Application application, CancellationToken ct = default)
    {
        const string sql = @"
            UPDATE applications SET
                waybill = @WayBill, client_id = @ClientId, car_id = @CarId,
                volume = @Volume, fact_volume = @FactVolume, volume_current = @VolumeCurrent,
                start_time = @StartTime, end_time = @EndTime, is_completed = @IsCompleted,
                is_deleted = @IsDeleted, user_id = @UserId, mixer_number = @MixerNumber,
                order_num = @Order, batch_phase = @BatchPhase, train_mode = @TrainMode,
                is_speed_app = @IsSpeedApp, last_save_batch_num = @LastSaveBatchNum,
                is_running = @IsRunning, is_edit_lock = @IsEditLock, product_id = @ProductId,
                unloading_point = @UnloadingPoint, correct_water_panel = @CorrectWaterPanel,
                start_auto = @StartAuto
            WHERE id = @Id";
        
        using var conn = _database.GetConnection();
        await conn.ExecuteAsync(sql, application);
    }
    
    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        const string sql = "UPDATE applications SET is_deleted = 1 WHERE id = @Id";
        using var conn = _database.GetConnection();
        await conn.ExecuteAsync(sql, new { Id = id });
    }
    
    public async Task<Application?> GetNextInQueueAsync(int mixerNumber, CancellationToken ct = default)
    {
        const string sql = @"SELECT * FROM applications 
            WHERE mixer_number = @MixerNumber AND is_running = 0 AND is_completed = 0 AND is_deleted = 0
            ORDER BY order_num LIMIT 1";
        using var conn = _database.GetConnection();
        return await conn.QueryFirstOrDefaultAsync<Application>(sql, new { MixerNumber = mixerNumber });
    }
}

public class RecipeRepository : IRecipeRepository
{
    private readonly ISmartMixDatabase _database;
    private readonly ILogger<RecipeRepository> _logger;
    
    public RecipeRepository(ISmartMixDatabase database, ILogger<RecipeRepository> logger)
    {
        _database = database;
        _logger = logger;
    }
    
    public async Task<Recipe?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        const string sql = "SELECT * FROM recipes WHERE id = @Id";
        using var conn = _database.GetConnection();
        return await conn.QueryFirstOrDefaultAsync<Recipe>(sql, new { Id = id });
    }
    
    public async Task<IReadOnlyList<Recipe>> GetAllAsync(CancellationToken ct = default)
    {
        const string sql = "SELECT * FROM recipes WHERE consider = 1 ORDER BY name";
        using var conn = _database.GetConnection();
        return (await conn.QueryAsync<Recipe>(sql)).ToList();
    }
    
    public async Task<IReadOnlyList<Recipe>> GetByCategoryAsync(int categoryId, CancellationToken ct = default)
    {
        const string sql = "SELECT * FROM recipes WHERE category_id = @CategoryId AND consider = 1 ORDER BY name";
        using var conn = _database.GetConnection();
        return (await conn.QueryAsync<Recipe>(sql, new { CategoryId = categoryId })).ToList();
    }
    
    public async Task<int> CreateAsync(Recipe recipe, CancellationToken ct = default)
    {
        const string sql = @"
            INSERT INTO recipes (name, consider, use_auto_correction, user_id, edit_date,
                time_set_id, category_id, mixer_set_id)
            VALUES (@Name, @Consider, @UseAutoCorrection, @UserId, @EditDate,
                @TimeSetId, @CategoryId, @MixerSetId);
            SELECT LAST_INSERT_ID();";
        
        using var conn = _database.GetConnection();
        return await conn.ExecuteScalarAsync<int>(sql, recipe);
    }
    
    public async Task UpdateAsync(Recipe recipe, CancellationToken ct = default)
    {
        const string sql = @"
            UPDATE recipes SET
                name = @Name, consider = @Consider, use_auto_correction = @UseAutoCorrection,
                user_id = @UserId, edit_date = @EditDate, time_set_id = @TimeSetId,
                category_id = @CategoryId, mixer_set_id = @MixerSetId
            WHERE id = @Id";
        
        using var conn = _database.GetConnection();
        await conn.ExecuteAsync(sql, recipe);
    }
    
    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        const string sql = "UPDATE recipes SET consider = 0 WHERE id = @Id";
        using var conn = _database.GetConnection();
        await conn.ExecuteAsync(sql, new { Id = id });
    }
    
    public async Task<bool> ExistsAsync(int id, CancellationToken ct = default)
    {
        const string sql = "SELECT COUNT(1) FROM recipes WHERE id = @Id";
        using var conn = _database.GetConnection();
        return await conn.ExecuteScalarAsync<int>(sql, new { Id = id }) > 0;
    }
}

public class BunkerRepository : IBunkerRepository
{
    private readonly ISmartMixDatabase _database;
    private readonly ILogger<BunkerRepository> _logger;
    
    public BunkerRepository(ISmartMixDatabase database, ILogger<BunkerRepository> logger)
    {
        _database = database;
        _logger = logger;
    }
    
    public async Task<Bunker?> GetByNumberAsync(int bunkerNumber, int lineNumber, CancellationToken ct = default)
    {
        const string sql = "SELECT * FROM bunkers WHERE number = @Number AND line_number = @LineNumber";
        using var conn = _database.GetConnection();
        return await conn.QueryFirstOrDefaultAsync<Bunker>(sql, new { Number = bunkerNumber, LineNumber = lineNumber });
    }
    
    public async Task<IReadOnlyList<Bunker>> GetAllAsync(int lineNumber, CancellationToken ct = default)
    {
        const string sql = "SELECT * FROM bunkers WHERE line_number = @LineNumber ORDER BY number";
        using var conn = _database.GetConnection();
        return (await conn.QueryAsync<Bunker>(sql, new { LineNumber = lineNumber })).ToList();
    }
    
    public async Task UpdateAsync(Bunker bunker, CancellationToken ct = default)
    {
        const string sql = @"
            UPDATE bunkers SET
                is_on = @IsOn, component_id = @ComponentId
            WHERE id = @Id";
        
        using var conn = _database.GetConnection();
        await conn.ExecuteAsync(sql, bunker);
    }
    
    public async Task<Bunker?> GetByMaterialAsync(int materialId, int lineNumber, CancellationToken ct = default)
    {
        const string sql = "SELECT * FROM bunkers WHERE component_id = @MaterialId AND line_number = @LineNumber AND is_on = 1";
        using var conn = _database.GetConnection();
        return await conn.QueryFirstOrDefaultAsync<Bunker>(sql, new { MaterialId = materialId, LineNumber = lineNumber });
    }
}

public class ComponentRepository : IComponentRepository
{
    private readonly ISmartMixDatabase _database;
    private readonly ILogger<ComponentRepository> _logger;
    
    public ComponentRepository(ISmartMixDatabase database, ILogger<ComponentRepository> logger)
    {
        _database = database;
        _logger = logger;
    }
    
    public async Task<Component?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        const string sql = "SELECT * FROM components WHERE id = @Id";
        using var conn = _database.GetConnection();
        return await conn.QueryFirstOrDefaultAsync<Component>(sql, new { Id = id });
    }
    
    public async Task<IReadOnlyList<Component>> GetAllAsync(CancellationToken ct = default)
    {
        const string sql = "SELECT * FROM components ORDER BY name";
        using var conn = _database.GetConnection();
        return (await conn.QueryAsync<Component>(sql)).ToList();
    }
    
    public async Task<IReadOnlyList<Component>> GetByTypeAsync(ComponentType type, CancellationToken ct = default)
    {
        const string sql = "SELECT * FROM components WHERE type = @Type ORDER BY name";
        using var conn = _database.GetConnection();
        return (await conn.QueryAsync<Component>(sql, new { Type = (int)type })).ToList();
    }
}

public class UserRepository : IUserRepository
{
    private readonly ISmartMixDatabase _database;
    private readonly ILogger<UserRepository> _logger;
    
    public UserRepository(ISmartMixDatabase database, ILogger<UserRepository> logger)
    {
        _database = database;
        _logger = logger;
    }
    
    public async Task<User?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        const string sql = "SELECT * FROM users WHERE id = @Id";
        using var conn = _database.GetConnection();
        return await conn.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
    }
    
    public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
    {
        const string sql = "SELECT * FROM users WHERE username = @Username";
        using var conn = _database.GetConnection();
        return await conn.QueryFirstOrDefaultAsync<User>(sql, new { Username = username });
    }
    
    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct = default)
    {
        const string sql = "SELECT * FROM users ORDER BY name";
        using var conn = _database.GetConnection();
        return (await conn.QueryAsync<User>(sql)).ToList();
    }
    
    public async Task<int> CreateAsync(User user, CancellationToken ct = default)
    {
        const string sql = @"
            INSERT INTO users (username, password_hash, name, role, is_active, created_at)
            VALUES (@Username, @PasswordHash, @Name, @Role, @IsActive, @CreatedAt);
            SELECT LAST_INSERT_ID();";
        
        using var conn = _database.GetConnection();
        return await conn.ExecuteScalarAsync<int>(sql, user);
    }
    
    public async Task UpdateAsync(User user, CancellationToken ct = default)
    {
        const string sql = @"
            UPDATE users SET
                username = @Username, password_hash = @PasswordHash, name = @Name,
                role = @Role, is_active = @IsActive
            WHERE id = @Id";
        
        using var conn = _database.GetConnection();
        await conn.ExecuteAsync(sql, user);
    }
}

public class ReportRepository : IReportRepository
{
    private readonly ISmartMixDatabase _database;
    private readonly ILogger<ReportRepository> _logger;
    
    public ReportRepository(ISmartMixDatabase database, ILogger<ReportRepository> logger)
    {
        _database = database;
        _logger = logger;
    }
    
    public async Task<ConsumptionReport> GetConsumptionReportAsync(DateTime from, DateTime to, int? lineNumber = null, CancellationToken ct = default)
    {
        // Реализация через хранимые процедуры или сложные запросы
        return new ConsumptionReport { From = from, To = to };
    }
    
    public async Task<ApplicationReport> GetApplicationReportAsync(int applicationId, CancellationToken ct = default)
    {
        return new ApplicationReport();
    }
    
    public async Task<BunkerConsumptionReport> GetBunkerConsumptionReportAsync(int bunkerNumber, DateTime from, DateTime to, CancellationToken ct = default)
    {
        return new BunkerConsumptionReport();
    }
}