using SmartMix.Core.Infrastructure.Persistence.Database.Models;
using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace SmartMix.Core.Infrastructure.Persistence.Database.Base
{
    /// <summary>
    /// Представляет базовый класс доступа к базе данных.
    /// </summary>
    public abstract class DatabaseBase<TConnection> where TConnection : DbConnection, new()
    {
        /// <summary>
        /// Представляет таймаут ожидания ответа от SQL Server, в секундах.
        /// </summary>
        private int _commandTimeout = 300;

        protected DbOperationResult<DataTable> GetData(TConnection connection, DbDataAdapter dataAdapter)
        {
#if DEBUG
            DebugWrite(dataAdapter.SelectCommand);
#endif
            try
            {
                connection.Open();

                DataSet answer = new DataSet();
                dataAdapter.SelectCommand.Connection = connection;
                dataAdapter.Fill(answer, "Answer");
                return new DbOperationResult<DataTable>(answer.Tables[0]);
            }
            catch (Exception ex)
            {
                return new DbOperationResult<DataTable>(ex);
            }
            finally
            {
                dataAdapter?.Dispose();
            }
        }

        /// <summary>
        /// Запрашивает данные для указанного подключения и команды. Возвращает результат выполнения операции.
        /// </summary>
        /// <param name="connection">Соединение с БД.</param>
        /// <param name="command">Команда SQL.</param>
        /// <returns>Список записей, удовлетворяющих запросу команды.</returns>
        protected DbOperationResult<List<DbDataRecord>> GetData(TConnection connection, DbCommand command)
        {
#if DEBUG
            DebugWrite(command);
#endif
            try
            {
                connection.Open();

                command.Connection = connection;
                command.CommandTimeout = _commandTimeout;

                var dataList = new List<DbDataRecord>();
                using (DbDataReader dataReader = command.ExecuteReader())
                {
                    if (dataReader.HasRows)
                    {
                        foreach (DbDataRecord record in dataReader)
                            dataList.Add(record);
                    }
                }
                return new DbOperationResult<List<DbDataRecord>>(dataList);
            }
            catch (Exception ex)
            {
                return new DbOperationResult<List<DbDataRecord>>(new List<DbDataRecord>(), ex);
            }
            finally
            {
                connection?.Dispose();
            }
        }

        /// <summary>
        /// Запрашивает значение для указанного подключения и команды. Возвращает результат выполнения операции.
        /// </summary>
        /// <param name="connection">Соединение с БД.</param>
        /// <param name="command">Команда SQL.</param>
        /// <returns>Значение первого столбца.</returns>
        protected DbOperationResult<object> ExecuteQuery(TConnection connection, DbCommand command)
        {

#if DEBUG
            DebugWrite(command);
#endif
            try
            {
                connection.Open();

                command.Connection = connection;
                object rc = command.ExecuteScalar();

                return new DbOperationResult<object>(rc);
            }
            catch (Exception ex)
            {
                return new DbOperationResult<object>(ex);
            }
        }

        /// <summary>
        /// Выполняет запрос на изменение БД. Возвращает количество обработанных строк.
        /// </summary>
        /// <param name="connection">Соединение с БД.</param>
        /// <param name="command">Команда SQL.</param>
        /// <returns>Количество обработанных строк.</returns>
        protected DbOperationResult<int> ChangeData(TConnection connection, DbCommand command)
        {
#if DEBUG
            DebugWrite(command);
#endif
            try
            {
                connection.Open();

                command.Connection = connection;
                int rc = command.ExecuteNonQuery();

                return new DbOperationResult<int>(rc);
            }
            catch (Exception ex)
            {
                return new DbOperationResult<int>(ex);
            }
        }

        /// <summary>
        /// Выполняет запрос на изменение БД в одной транзакции для указанного набора команд. Возвращает результат выполнения операции.
        /// </summary>
        /// <param name="connection">Соединение с БД.</param>
        /// <param name="commands">Массив команд.</param>
        /// <returns>Ответ сервера со значением <see langword="true"/>, если операция выполнена успешно, иначе - со значением <see langword="false"/>.</returns>
        protected DbOperationResult<bool> ChangeData(TConnection connection, DbCommand[] commands)
        {
            try
            {
                connection.Open();
            }
            catch (Exception ex)
            {
                return new DbOperationResult<bool>($"Ошибка соединения с БД: {ex.GetBaseException().Message}", ex);
            }

            DbTransaction transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);
            foreach (DbCommand command in commands)
            {
                command.Connection = connection;
                command.Transaction = transaction;
            }

            int i = 0;
            try
            {
                for (; i < commands.Length; i++)
                    commands[i].ExecuteNonQuery();

                transaction.Commit();
                return new DbOperationResult<bool>(true);
            }
            catch (Exception ex)
            {
                try
                {
                    transaction.Rollback();
                    return new DbOperationResult<bool>($"Ошибка обновления данных в одной транзакции. Номер команды: #{i}: {ex.GetBaseException().Message}", ex);
                }
                catch (Exception exR)
                {
                    return new DbOperationResult<bool>($"Ошибка обновления данных в одной транзакции. Номер команды: #{i}: {ex.GetBaseException().Message}\r\nRollback Exception: {exR.Message}", ex);
                }
            }
            finally
            {
                connection.Dispose();
            }
        }

        /// <summary>
        /// Выполняет обновление БД в одной транзакции по указанным параметрам. Возвращает результат выполнения операции.
        /// </summary>
        /// <param name="connection">Соединение с БД.</param>
        /// <param name="commands">Словарь команд, где ключом является уникальный GUID, а значением - SQL-команда.</param>
        /// <returns>Словарь, где ключом является  уникальный GUID команды, а значением - количество обработанных строк.</returns>
        protected DbOperationResult<Dictionary<Guid, int>> ChangeData(TConnection connection, Dictionary<Guid, DbCommand> commands)
        {
            var result = new Dictionary<Guid, int>();
            try
            {
                connection.Open();
            }
            catch (Exception ex)
            {
                return new DbOperationResult<Dictionary<Guid, int>>(new Dictionary<Guid, int>(), $"Ошибка соединения с БД: {ex.GetBaseException().Message}", ex);
            }

            DbTransaction transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);
            foreach (KeyValuePair<Guid, DbCommand> command in commands)
            {
                command.Value.Connection = connection;
                command.Value.Transaction = transaction;
            }

            int i = 0;
            try
            {
                foreach (KeyValuePair<Guid, DbCommand> command in commands)
                {
                    int temp = command.Value.ExecuteNonQuery();
                    result.Add(command.Key, temp);
                    i++;
                }

                transaction.Commit();
                return new DbOperationResult<Dictionary<Guid, int>>(result);
            }
            catch (Exception ex)
            {
                try
                {
                    transaction.Rollback();
                    return new DbOperationResult<Dictionary<Guid, int>>(new Dictionary<Guid, int>(), $"Ошибка обновления данных в одной транзакции. Номер команды: #{i}: {ex.GetBaseException().Message}", ex);
                }
                catch (Exception exR)
                {
                    return new DbOperationResult<Dictionary<Guid, int>>(new Dictionary<Guid, int>(), $"Ошибка обновления данных в одной транзакции. Номер команды: #{i}: {ex.GetBaseException().Message}\r\nRollback Exception: {exR.Message}", ex);
                }
            }
        }

        /// <summary>
        /// Запрашивает данные по указанным параметрам. Возвращает результат выполнения операции.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="command"></param>
        /// <param name="methodName"></param>
        /// <returns>Ответ сервера со списком записей.</returns>
        protected async Task<DbOperationResult<List<DbDataRecord>>> GetDataAsync(TConnection connection, DbCommand command, [CallerMemberName] string methodName = null)
        {
#if DEBUG
            DebugWrite(command);
#endif
            try
            {
                connection.Open();

                command.Connection = connection;
                command.CommandTimeout = _commandTimeout;

                var dataList = new List<DbDataRecord>();
                using (DbDataReader dataReader = await command.ExecuteReaderAsync())
                {
                    if (dataReader.HasRows)
                    {
                        foreach (DbDataRecord record in dataReader)
                            dataList.Add(record);
                    }
                }
                return new DbOperationResult<List<DbDataRecord>>(dataList);
            }
            catch (Exception ex)
            {
                return new DbOperationResult<List<DbDataRecord>>(new List<DbDataRecord>(), $"[{methodName}] Ошибка при запросе данных: {ex.GetBaseException().Message}", ex);
            }
        }

        /// <summary>
        /// Выполняет обновление БД по указанным параметрам. Возвращает число обработанных строк.
        /// </summary>
        /// <param name="connection">Соединение с БД.</param>
        /// <param name="command">Команда SQL/</param>
        /// <param name="methodName"></param>
        /// <returns>Ответ сервера с количеством обработанных строк.</returns>
        protected async Task<DbOperationResult<int>> ChangeDataAsync(TConnection connection, DbCommand command, [CallerMemberName] string methodName = null)
        {
#if DEBUG
            DebugWrite(command);
#endif
            try
            {
                connection.Open();

                command.Connection = connection;
                int response = await command.ExecuteNonQueryAsync();

                return new DbOperationResult<int>(response);
            }
            catch (Exception ex)
            {
                return new DbOperationResult<int>($"[{methodName}] Ошибка изменения данных: {ex.GetBaseException().Message}", ex);
            }
        }

        /// <summary>
        /// Вывод запроса в консоль MS Studio.
        /// </summary>
        /// <param name="command">Команда SQL/</param>
        protected void DebugWrite(DbCommand command) => Debug.WriteLine($"[{DateTime.Now}] {command.CommandText.Replace('\n', ' ')}");
    }
}
