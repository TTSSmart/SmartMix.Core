using Dapper;
using MySql.Data.MySqlClient;
using SmartMix.Core.Domain.Entities;
using SmartMix.Core.Domain.Interfaces;


namespace SmartMix.Core.Infrastructure.Persistence.Repositories.Repositories
{
    public class ClientRepository : IClientRepository
    {
        private readonly string _connectionString;

        public ClientRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<Client>> GetAllAsync(CancellationToken ct = default)
        {
            const string sql = "SELECT id_client AS Id, name AS Name, address AS Address FROM client";
            using var connection = new MySqlConnection(_connectionString);
            return await connection.QueryAsync<Client>(sql);
            // Если нужно исключить "не выбран" (Id = 0), добавь: WHERE id_client > 0
        }

        public async Task<Client?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            const string sql = "SELECT id_client AS Id, name AS Name, address AS Address FROM client WHERE id_client = @Id";
            using var connection = new MySqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<Client>(sql, new { Id = id });
        }

        public async Task<Client?> GetByNameAsync(string name, CancellationToken ct = default)
        {
            const string sql = "SELECT id_client AS Id, name AS Name, address AS Address FROM client WHERE name = @Name";
            using var connection = new MySqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<Client>(sql, new { Name = name });
        }

        public async Task<IEnumerable<Client>> GetArchiveByIdsAsync(IEnumerable<int> ids, int lineNumber, CancellationToken ct = default)
        {
            if (!ids.Any()) return Enumerable.Empty<Client>();

            // ВАЖНО: Защита от SQL-инъекции при динамическом имени таблицы
            if (lineNumber != 1 && lineNumber != 2)
                throw new ArgumentException("Недопустимый номер линии");

            string tableName = $"l{lineNumber}_archive_client";

            // Dapper автоматически и безопасно преобразует IEnumerable<int> в список параметров для IN
            string sql = $@"
            SELECT id_client AS Id, name AS Name, address AS Address, old_id AS OldId 
            FROM {tableName} 
            WHERE id_client IN @Ids";

            using var connection = new MySqlConnection(_connectionString);
            return await connection.QueryAsync<Client>(sql, new { Ids = ids });
        }

        public async Task<int> CopyToArchiveAsync(int clientId, int lineNumber, CancellationToken ct = default)
        {
            if (lineNumber != 1 && lineNumber != 2) throw new ArgumentException("Недопустимый номер линии");
            string tableName = $"l{lineNumber}_archive_client";

            using var connection = new MySqlConnection(_connectionString);

            // 1. Получаем клиента
            var client = await GetByIdAsync(clientId, ct);
            if (client == null) return clientId;

            // 2. Проверяем, есть ли он уже в архиве (по имени и адресу, как в оригинале)
            string checkSql = $"SELECT id_client FROM {tableName} WHERE name = @Name AND address = @Address LIMIT 1";
            var existingId = await connection.QueryFirstOrDefaultAsync<int?>(checkSql, client);

            if (existingId.HasValue) return existingId.Value;

            // 3. Копируем в архив
            string insertSql = $@"
            INSERT INTO {tableName} (name, address, old_id) 
            SELECT name, address, id_client FROM client WHERE id_client = @Id; 
            SELECT LAST_INSERT_ID();";

            return await connection.QuerySingleAsync<int>(insertSql, new { Id = clientId });
        }

        public async Task<int> AddAsync(Client client, CancellationToken ct = default)
        {
            const string sql = @"
            INSERT INTO client (name, address) 
            VALUES (@Name, @Address); 
            SELECT LAST_INSERT_ID();";

            using var connection = new MySqlConnection(_connectionString);
            return await connection.QuerySingleAsync<int>(sql, client);
        }

        public async Task<bool> UpdateAsync(Client client, CancellationToken ct = default)
        {
            const string sql = @"
            UPDATE client 
            SET name = @Name, address = @Address 
            WHERE id_client = @Id";

            using var connection = new MySqlConnection(_connectionString);
            int affectedRows = await connection.ExecuteAsync(sql, client);
            return affectedRows > 0;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            if (id <= 0) return false; // Защита от удаления системных записей (Id = 0)

            const string sql = "DELETE FROM client WHERE id_client = @Id";
            using var connection = new MySqlConnection(_connectionString);
            int affectedRows = await connection.ExecuteAsync(sql, new { Id = id });
            return affectedRows > 0;
        }

        public async Task<IEnumerable<int>> AddOrUpdateBatchAsync(IEnumerable<Client> clients, CancellationToken ct = default)
        {
            var resultIds = new List<int>();

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync(ct);

            // Явная транзакция: либо все клиенты сохранятся, либо ни один
            using var transaction = await connection.BeginTransactionAsync(ct);

            try
            {
                foreach (var client in clients)
                {
                    if (client.Id > 0) // Обновление существующего
                    {
                        const string updateSql = "UPDATE client SET name = @Name, address = @Address WHERE id_client = @Id";
                        await connection.ExecuteAsync(updateSql, client, transaction);
                        resultIds.Add(client.Id);
                    }
                    else // Добавление нового (Id == 0 или -1)
                    {
                        const string insertSql = @"
                        INSERT INTO client (name, address) 
                        VALUES (@Name, @Address); 
                        SELECT LAST_INSERT_ID();";

                        client.Id = await connection.QuerySingleAsync<int>(insertSql, client, transaction);
                        resultIds.Add(client.Id);
                    }
                }

                await transaction.CommitAsync(ct);
                return resultIds;
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw; // Пробрасываем исключение на уровень Application для логирования
            }
        }
    }
}
