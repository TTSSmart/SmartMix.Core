using Dapper;
using MySql.Data.MySqlClient;
using SmartMix.Core.Domain.Entities;
using SmartMix.Core.Domain.Interfaces;

namespace SmartMix.Core.Infrastructure.Persistence.Repositories.Repositories
{
    internal class CarRepository : ICarRepository
    {
        private readonly string _connectionString;

        public CarRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<Car>> GetAllAsync(CancellationToken ct = default)
        {
            const string sql = @"
            SELECT id, id_car AS Name, model AS Model, driver AS Driver, volume AS Volume 
            FROM cars";

            using var connection = new MySqlConnection(_connectionString);
            // Dapper САМ создаст объекты Car, сопоставив колонки (благодаря AS Name)
            return await connection.QueryAsync<Car>(sql);
        }

        public async Task<Car?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            const string sql = @"
            SELECT id, id_car AS Name, model AS Model, driver AS Driver, volume AS Volume 
            FROM cars WHERE id = @Id";

            using var connection = new MySqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<Car>(sql, new { Id = id });
        }

        public async Task<Car?> GetByNumberAsync(string carNumber, CancellationToken ct = default)
        {
            const string sql = @"
            SELECT id, id_car AS Name, model AS Model, driver AS Driver, volume AS Volume 
            FROM cars WHERE id_car = @CarNumber";

            using var connection = new MySqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<Car>(sql, new { CarNumber = carNumber });
        }

        public async Task<IEnumerable<Car>> GetArchiveByIdsAsync(IEnumerable<int> ids, int lineNumber, CancellationToken ct = default)
        {
            if (!ids.Any()) return Enumerable.Empty<Car>();

            // ВАЖНО: lineNumber должен быть валидирован (например, 1 или 2), 
            // так как имена таблиц нельзя параметризовать через @param
            if (lineNumber != 1 && lineNumber != 2)
                throw new ArgumentException("Недопустимый номер линии");

            string tableName = $"l{lineNumber}_archive_cars";

            string sql = $@"
            SELECT id, id_car AS Name, model AS Model, driver AS Driver, volume AS Volume, old_id AS OldId 
            FROM {tableName} 
            WHERE id IN @Ids";

            using var connection = new MySqlConnection(_connectionString);
            return await connection.QueryAsync<Car>(sql, new { Ids = ids });
        }

        public async Task<int> CopyToArchiveAsync(int carId, int lineNumber, CancellationToken ct = default)
        {
            if (lineNumber != 1 && lineNumber != 2) throw new ArgumentException("Недопустимый номер линии");
            string tableName = $"l{lineNumber}_archive_cars";

            using var connection = new MySqlConnection(_connectionString);

            // 1. Получаем машину
            var car = await GetByIdAsync(carId, ct);
            if (car == null) return carId;

            // 2. Проверяем, есть ли она уже в архиве (по номеру и объему)
            string checkSql = $"SELECT id FROM {tableName} WHERE id_car = @Name AND volume = @Volume LIMIT 1";
            var existingId = await connection.QueryFirstOrDefaultAsync<int?>(checkSql, car);

            if (existingId.HasValue) return existingId.Value;

            // 3. Копируем в архив
            string insertSql = $@"
            INSERT INTO {tableName} (id_car, volume, model, driver, old_id) 
            VALUES (@Name, @Volume, @Model, @Driver, @Id); 
            SELECT LAST_INSERT_ID();";

            return await connection.QuerySingleAsync<int>(insertSql, car);
        }

        public async Task<IEnumerable<int>> AddOrUpdateBatchAsync(IEnumerable<Car> cars, CancellationToken ct = default)
        {
            var resultIds = new List<int>();

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync(ct);

            // Явная транзакция: либо все машины сохранятся, либо ни одна (откат при ошибке)
            using var transaction = await connection.BeginTransactionAsync(ct);

            try
            {
                foreach (var car in cars)
                {
                    if (car.Id > 0) // Обновление
                    {
                        const string updateSql = @"
                        UPDATE cars SET id_car = @Name, volume = @Volume, model = @Model, driver = @Driver 
                        WHERE id = @Id";
                        await connection.ExecuteAsync(updateSql, car, transaction);
                        resultIds.Add(car.Id);
                    }
                    else // Вставка
                    {
                        const string insertSql = @"
                        INSERT INTO cars (id_car, model, driver, volume) 
                        VALUES (@Name, @Model, @Driver, @Volume); 
                        SELECT LAST_INSERT_ID();";

                        car.Id = await connection.QuerySingleAsync<int>(insertSql, car, transaction);
                        resultIds.Add(car.Id);
                    }
                }

                await transaction.CommitAsync(ct);
                return resultIds;
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw; // Пробрасываем исключение выше, пусть Global Exception Handler или Application слой логирует
            }
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            if (id == 0) return false; // Защита от удаления "не выбрано"

            const string sql = "DELETE FROM cars WHERE id = @Id";
            using var connection = new MySqlConnection(_connectionString);

            int affectedRows = await connection.ExecuteAsync(sql, new { Id = id });
            return affectedRows > 0;
        }

        public Task<int> AddAsync(Car car, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateAsync(Car car, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
