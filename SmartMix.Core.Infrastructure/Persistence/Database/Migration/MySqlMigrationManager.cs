using SmartMix.Core.Infrastructure.Persistence.Database.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace SmartMix.Core.Infrastructure.Persistence.Database.Migration
{
    /// <summary>
    /// Представляет класс описания менеджера миграций баз данных MySQL.
    /// </summary>
    public class MySqlMigrationManager : MigrationManagerBase<MySqlConnection, MySQLDatabase>
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса
        /// </summary>
        /// <param name="db">Адаптер БД.</param>
        /// <param name="rm">Менеджер ресурсов файлов обновления БД.</param>
        public MySqlMigrationManager(MySQLDatabase db, ResourceManager rm) : base(db, rm)
        {
        }

        /// <summary>
        /// Выполняет установку обновлений на БД.
        /// </summary>
        /// <exception cref="Exception">Исключение, которое генерируется в процессе ошибки выполнения операции.</exception>
        public override void Migrate()
        {
            MySqlCommand command = new MySqlCommand($"SHOW TABLES LIKE '{DbTableName}'");
            DbOperationResult<List<DbDataRecord>> getResult = Db.GetData(command);
            if (getResult.Success && getResult.Result.Count == 0)
            {
                // создаём таблицу миграций
                DbOperationResult<object> createTableResult = Db.ExecuteQuery(new MySqlCommand(Resources.create_migration_table));
                if (!createTableResult.Success) throw new Exception($"Не удалось создать таблицу миграции <{DbTableName}>: {createTableResult.Error}", createTableResult.Exception);
            }

            // разбираем миграции
            NewMigration[] newMigrations = GetMigrations();
            DbOperationResult<object> appliedResult = Db.ExecuteQuery(new MySqlCommand($"SELECT IFNULL(MAX(id),-1) FROM {DbTableName}"));
            if (!appliedResult.Success || !long.TryParse(appliedResult.Result.ToString(), out long max))
                throw new Exception($"Ошибка запроса примененных миграций: {appliedResult.Error}", appliedResult.Exception);

            foreach (NewMigration item in newMigrations)
            {
                if (item.Id > max)
                    ApplyMigration(item, DateTime.Now);
            }
        }

        /// <summary>
        /// Накатывает на БД указанную миграцию.
        /// </summary>
        /// <param name="migration">Миграция БД.</param>
        /// <param name="applied">Дата применения.</param>
        /// <exception cref="Exception">Исключение, которое генерируется в процессе ошибки выполнения операции.</exception>
        private void ApplyMigration(NewMigration migration, DateTime applied)
        {
            try
            {
                try
                {
                    // по- новому: нужно, когда накатываются новые хранимые процедуры из-за проблем с delimiter
                    // требуются права администратора
                    using (var connection = new MySqlConnection(Db.ConnectionString))
                    {
                        var script = new MySqlScript(connection, migration.Sql);
                        connection.Open();
                        script.Execute();
                    }
                }
                catch (Exception)
                {
                    //по - старому
                    MySqlCommand query = new MySqlCommand(migration.Sql);
                    DbOperationResult<object> resQuery = Db.ExecuteQuery(query);

                    if (!resQuery.Success)
                        throw new Exception(resQuery.Error, resQuery.Exception);

                    /* 02/07/2025 Ольга todo нужно посмотреть, даст ли выигрыш разделение на подзапросы
                    var queryes = migration.Sql.Split(';').Where(s => !string.IsNullOrEmpty(s.Trim())).Select(qu => new MySqlCommand(qu));

                    var resExecute = Db.ChangeData(queryes.ToArray());
                    if (!resExecute.Success) throw new Exception($"Миграция {migration.Id} - {migration.Name} завершилась с ошибкой: {resExecute.ExceptionMessage}");
                    */
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Ошибка применения миграции {0}: {1}", migration, e);
                throw new Exception($"Не удалось установить миграцию {migration}", e);
            }

            // фиксируем её установку
            MySqlCommand command = new MySqlCommand($"INSERT INTO `{DbTableName}` VALUES (@idMigr, @nameMigr, @timeMigr);");
            command.Parameters.AddWithValue("@idMigr", migration.Id);
            command.Parameters.AddWithValue("@nameMigr", migration.Name);
            command.Parameters.Add(new MySqlParameter("@timeMigr", MySqlDbType.DateTime) { Value = applied });

            Db.ExecuteQuery(command);
            Console.WriteLine($"Миграция успешно применена: {migration}");
        }

        /// <summary>
        /// Получает список текущих миграций
        /// </summary>
        /// <returns>Набор миграций SQL-сервера.</returns>
        private NewMigration[] GetMigrations()
        {
            var migrations = new List<NewMigration>();

            ResourceSet resourceSet = Resource.GetResourceSet(CultureInfo.CurrentUICulture, true, true);
            foreach (DictionaryEntry entry in resourceSet)
            {
                string[] s = entry.Key.ToString().Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
                if (s.Length <= 1) continue;
                if (!long.TryParse(s[0], out long id)) continue; // todo в логи
                if (string.IsNullOrWhiteSpace(s[1])) continue;

                var sqlObject = entry.Value;
                string sqlString = entry.Value.ToString();

                if (sqlObject is byte[] scriptBytes)
                    sqlString = Encoding.GetEncoding(1251).GetString(scriptBytes);

                if (string.IsNullOrWhiteSpace(sqlString)) continue;

                migrations.Add(new NewMigration
                {
                    Id = id,
                    Name = entry.Key.ToString().Replace($"{s[0]}_", "").Trim(),
                    Sql = sqlString
                });
            }
            return migrations.OrderBy(m => m.Id).ToArray();
        }
    }
}
