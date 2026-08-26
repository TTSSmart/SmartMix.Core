using SmartMix.Core.Infrastructure.Persistence.Database.Extensions;
using SmartMix.Core.Infrastructure.Persistence.Database.Interface;
using SmartMix.Core.Infrastructure.Persistence.Database.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SmartMix.Core.Infrastructure.Persistence.Database.Base
{
    /// <summary>
    /// Представляет базовый класс доступа к базе данных SmartMix.
    /// </summary>
    /// <typeparam name="TConnection">Тип подключения.</typeparam>
    public abstract class SmartMixDatabaseBase<TConnection> : DatabaseBase<TConnection>, ISmartMixDatabase //, IDisposable
        where TConnection : DbConnection, new()
    {
        /// <summary>Строка подключения к БД.</summary>
        private readonly string _connectionString;

        /// <summary>
        /// Представляет таймаут ожидания ответа от SQL Server, в секундах.
        /// </summary>
        private int _commandTimeout = 300;

        /// <summary>
        /// Инициализирует новый экземпляр класса для указанной строки подключения.
        /// </summary>
        /// <param name="connectionString">Строка подключения к БД.</param>
        /// <exception cref="ArgumentNullException">Исключение, которое генерируется, если не указана строка подключения к БД.</exception>
        public SmartMixDatabaseBase(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentNullException(nameof(connectionString), "Не задано подключение к базе данных");

            _connectionString = connectionString;
        }

        /// <summary>
        /// Возвращает строку подключения к БД.
        /// </summary>
        public string ConnectionString
        {
            get
            {
                return _connectionString;
            }
        }

        /// <summary>
        /// Выполняет проверку подключения к БД .Возвращает результат выполнения операции.
        /// </summary>
        /// <returns>Значение <see langword="true"/>, если соединение с БД было успешно открыто, иначе - значение <see langword="false"/>.</returns>
        public DbOperationResult<bool> CheckConnection()
        {
            try
            {
                using (TConnection conn = new TConnection() { ConnectionString = _connectionString })
                {
                    conn.Open();
                    return new DbOperationResult<bool>(conn.State == ConnectionState.Open);
                }
            }
            catch (Exception e)
            {
                return new DbOperationResult<bool>($"Ошибка соединения с БД: {e.GetBaseException().Message}", e);
            }

        }

        public DbOperationResult<DataTable> GetData(DbDataAdapter dataAdapter)
        {
            return ExecuteOperation(dataAdapter, GetData);
            //using (TConnection conn = new TConnection() { ConnectionString = _connectionString })
            //{
            //    return GetData(conn, dataAdapter);
            //}
        }

        public DbOperationResult<List<DbDataRecord>> GetData(DbCommand command)
        {
            return ExecuteOperation(command, GetData);
            //using (TConnection conn = new TConnection() { ConnectionString = _connectionString })
            //{
            //    return GetData(conn, command);
            //}
        }

        public DbOperationResult<object> ExecuteQuery(DbCommand command)
        {
            return ExecuteOperation(command, ExecuteQuery);
            //using (TConnection conn = new TConnection() { ConnectionString = _connectionString })
            //{
            //    return ExecuteQuery(conn, command);
            //}
        }

        public DbOperationResult<int> ChangeData(DbCommand command)
        {
            return ExecuteOperation(command, ChangeData);
            //using (TConnection conn = new TConnection() { ConnectionString = _connectionString })
            //{
            //    return ChangeData(conn, command);
            //}
        }

        /// <summary>
        /// Выполняет запрос на изменение БД в одной транзакции для указанного набора команд. Возвращает результат выполнения операции.
        /// </summary>
        /// <param name="commands">Массив команд.</param>
        /// <returns>Ответ сервера со значением <see langword="true"/>, если операция выполнена успешно, иначе - со значением <see langword="false"/>.</returns>
        public DbOperationResult<bool> ChangeData(DbCommand[] commands)
        {
            return ExecuteOperation(commands, ChangeData);
            //using (TConnection conn = new TConnection() { ConnectionString = _connectionString })
            //{
            //    return ChangeData(conn, commands);
            //}
        }

        public DbOperationResult<Dictionary<Guid, int>> ChangeData(Dictionary<Guid, DbCommand> commands)
        {
            return ExecuteOperation(commands, ChangeData);
            //using (TConnection conn = new TConnection() { ConnectionString = _connectionString })
            //{
            //    return ChangeData(conn, commands);
            //}
        }

        #region Универсальные методы
        protected DbOperationResult<T> ExecuteOperation<T>(Func<TConnection, DbOperationResult<T>> func)
        {
            using TConnection conn = new TConnection() { ConnectionString = _connectionString };
            return func.Invoke(conn);
        }
        protected DbOperationResult<T> ExecuteOperation<T>(DbCommand command, Func<TConnection, DbCommand, DbOperationResult<T>> func)
        {
            using TConnection conn = new TConnection() { ConnectionString = _connectionString };
            return func.Invoke(conn, command);
        }
        protected DbOperationResult<T> ExecuteOperation<T>(Dictionary<Guid, DbCommand> command, Func<TConnection, Dictionary<Guid, DbCommand>, DbOperationResult<T>> func)
        {
            using TConnection conn = new TConnection() { ConnectionString = _connectionString };
            return func.Invoke(conn, command);
        }
        protected DbOperationResult<T> ExecuteOperation<T>(DbCommand[] command, Func<TConnection, DbCommand[], DbOperationResult<T>> func)
        {
            using TConnection conn = new TConnection() { ConnectionString = _connectionString };
            return func.Invoke(conn, command);
        }
        protected DbOperationResult<T> ExecuteOperation<T>(DbDataAdapter dataAdapter, Func<TConnection, DbDataAdapter, DbOperationResult<T>> func)
        {
            using TConnection conn = new TConnection() { ConnectionString = _connectionString };
            return func.Invoke(conn, dataAdapter);
        }
        protected Task<DbOperationResult<List<DbDataRecord>>> ExecuteOperationAsync(DbCommand command, Func<TConnection, DbCommand, string, Task<DbOperationResult<List<DbDataRecord>>>> func, [CallerMemberName] string methodName = null)
        {
            using TConnection conn = new TConnection() { ConnectionString = _connectionString };
            return func.Invoke(conn, command, methodName);
        }
        protected Task<DbOperationResult<int>> ExecuteOperationAsync(DbCommand command, Func<TConnection, DbCommand, string, Task<DbOperationResult<int>>> func, [CallerMemberName] string methodName = null)
        {
            using TConnection conn = new TConnection() { ConnectionString = _connectionString };
            return func.Invoke(conn, command, methodName);
        }

        #endregion Универсальные методы

        #region Асинхронные обертки
        public virtual Task<DbOperationResult<bool>> CheckConnectionAsync(CancellationToken cancellationToken)
            => ExecuteAsync(CheckConnection, cancellationToken);

        public virtual Task<DbOperationResult<List<DbDataRecord>>> GetDataAsync(DbCommand command, CancellationToken cancellationToken)
            => ExecuteAsync(GetData, command, cancellationToken);

        public virtual Task<DbOperationResult<object>> ExecuteQueryAsync(DbCommand command, CancellationToken cancellationToken)
            => ExecuteAsync(ExecuteQuery, command, cancellationToken);

        public virtual Task<DbOperationResult<DataTable>> GetDataTableAsync(DbDataAdapter dataAdapter, CancellationToken cancellationToken)
            => ExecuteAsync(GetData, dataAdapter, cancellationToken);

        public virtual Task<DbOperationResult<int>> ChangeDataAsync(DbCommand command, CancellationToken cancellationToken)
            => ExecuteAsync(ChangeData, command, cancellationToken);

        public virtual Task<DbOperationResult<bool>> ChangeDataAsync(DbCommand[] command, CancellationToken cancellationToken)
            => ExecuteAsync(ChangeData, command, cancellationToken);

        public virtual Task<DbOperationResult<Dictionary<Guid, int>>> ChangeDataAsync(Dictionary<Guid, DbCommand> command, CancellationToken cancellationToken)
            => ExecuteAsync(ChangeData, command, cancellationToken);

        private Task<DbOperationResult<T>> ExecuteAsync<K, T>(Func<K, DbOperationResult<T>> func, K arg, CancellationToken cancellationToken)
        {
            TaskCompletionSource<DbOperationResult<T>> taskCompletionSource = new TaskCompletionSource<DbOperationResult<T>>();
            if (cancellationToken.IsCancellationRequested)
            {
                taskCompletionSource.SetCanceled();
            }
            else
            {
                try
                {
                    taskCompletionSource.SetResult(func.Invoke(arg));
                }
                catch (Exception ex)
                {
                    taskCompletionSource.SetException(ex);
                }
            }
            return taskCompletionSource.Task;
        }

        private Task<DbOperationResult<T>> ExecuteAsync<T>(Func<DbOperationResult<T>> func, CancellationToken cancellationToken)
        {
            TaskCompletionSource<DbOperationResult<T>> taskCompletionSource = new TaskCompletionSource<DbOperationResult<T>>();
            if (cancellationToken.IsCancellationRequested)
            {
                taskCompletionSource.SetCanceled();
            }
            else
            {
                try
                {
                    taskCompletionSource.SetResult(func.Invoke());
                }
                catch (Exception ex)
                {
                    taskCompletionSource.SetException(ex);
                }
            }
            return taskCompletionSource.Task;
        }

        #endregion Асинхронные обертки

        public virtual Task<DbOperationResult<IList<T>>> GetDataAsync<T>(DbCommand command, Func<DbDataReader, IList<T>> converterFunc, CancellationToken cancellationToken)
        {
            TaskCompletionSource<DbOperationResult<IList<T>>> taskCompletionSource = new TaskCompletionSource<DbOperationResult<IList<T>>>();
            if (cancellationToken.IsCancellationRequested)
            {
                taskCompletionSource.SetCanceled();
            }
            else
            {
                try
                {
                    DbOperationResult<IList<T>> response = GetDataSync(command, converterFunc);
                    taskCompletionSource.SetResult(response);
                }
                catch (Exception ex)
                {
                    string message = GetErrorFormat(command, ex.Message);
                    taskCompletionSource.SetException(new Exception(message));
                }
            }
            return taskCompletionSource.Task;
        }

        public virtual Task<DbOperationResult<T>> GetDataAsync<T>(DbCommand command, Func<DbDataReader, T> converterFunc, CancellationToken cancellationToken)
        {
            TaskCompletionSource<DbOperationResult<T>> taskCompletionSource = new TaskCompletionSource<DbOperationResult<T>>();
            if (cancellationToken.IsCancellationRequested)
            {
                taskCompletionSource.SetCanceled();
            }
            else
            {
                try
                {
                    DbOperationResult<T> response = GetDataSync(command, converterFunc);
                    taskCompletionSource.SetResult(response);
                }
                catch (Exception ex)
                {
                    string message = GetErrorFormat(command, ex.Message);
                    taskCompletionSource.SetException(new Exception(message));
                }
            }
            return taskCompletionSource.Task;
        }

        public virtual Task<DbOperationResult<T>> GetDataFirstAsync<T>(DbCommand command, Func<DbDataReader, IList<T>> converterFunc, CancellationToken cancellationToken, [CallerMemberName] string memberName = "")
        {
            TaskCompletionSource<DbOperationResult<T>> taskCompletionSource = new TaskCompletionSource<DbOperationResult<T>>();
            if (cancellationToken.IsCancellationRequested)
            {
                taskCompletionSource.SetCanceled();
            }
            else
            {
                try
                {
                    DbOperationResult<IList<T>> response = GetDataSync(command, converterFunc);
                    DbOperationResult<T> result;
                    if (response.Result == null)
                    {
                        result = new DbOperationResult<T>(GetErrorFormat(command, $"[{memberName}] -message-> {response.Error}"), response.Exception);
                    }
                    else
                    {
                        result = new DbOperationResult<T>(response.Result.FirstOrDefault());
                    }
                    taskCompletionSource.SetResult(result);
                }
                catch (Exception ex)
                {
                    string message = GetErrorFormat(command, ex.Message);
                    taskCompletionSource.SetException(new Exception(message));
                }
            }
            return taskCompletionSource.Task;
        }

        private DbOperationResult<IList<T>> GetDataSync<T>(DbCommand command, Func<DbDataReader, IList<T>> converterFunc)
        {
            using (TConnection connection = new TConnection() { ConnectionString = _connectionString })
            {
                connection.Open();

                command.Connection = connection;
                command.CommandTimeout = _commandTimeout;
                using (DbDataReader dataReader = command.ExecuteReader())
                {
                    IList<T> response = new List<T>();
                    if (dataReader.HasRows)
                        response = converterFunc.Invoke(dataReader);

                    return new DbOperationResult<IList<T>>(response);
                }
            }
        }

        private DbOperationResult<T> GetDataSync<T>(DbCommand command, Func<DbDataReader, T> converterFunc)
        {
            using (TConnection connection = new TConnection() { ConnectionString = _connectionString })
            {
                connection.Open();

                command.Connection = connection;
                command.CommandTimeout = _commandTimeout;

                using (DbDataReader dataReader = command.ExecuteReader())
                {
                    T response = default(T);
                    if (dataReader.HasRows)
                        response = converterFunc.Invoke(dataReader);

                    return new DbOperationResult<T>(response);
                }
            }
        }

        public DbCommand CreateCommand(string commandText)
        {
            TConnection connection = new TConnection() { ConnectionString = _connectionString };
            DbCommand command = connection.CreateCommand();
            command.CommandText = commandText;

            return command;
        }

        public abstract DataSet ExecuteProcedure(string procedureName, DbParameter[] parameters);
        public abstract DataSet ExecuteCommand(string cmdText, params DbParameter[] parameters);
        public abstract void ExecuteNonQuery(string cmdText, params DbParameter[] parameters);
        public abstract T ExecuteScalar<T>(string cmdText, params DbParameter[] parameters);

        private readonly Func<DbCommand, string, string> GetErrorFormat = (command, exMessage)
           => $"Команда: {command.CommandText}\nПараметры:{command.Parameters.ToForString()}\n{exMessage}";

        //#region IDisposable Members

        ///// <summary>
        ///// Разрывает соединение с БД.
        ///// </summary>
        ///// <param name="disposing">Значение <see langword="true"/>, если метод вызывается из Dispose(), значение <see langword="false"/>, если метод вызывается из метода завершения.</param>
        //protected void Dispose(bool disposing)
        //{
        //    if (disposing)
        //    {
        //        // Dispose managed resources
        //        //if (!_isOwner) return;

        //        try
        //        {
        //            if (_dbConnection != null)
        //            {
        //                //SqlConnection.ClearPool(SqlConn);
        //                _dbConnection.Close();
        //                _dbConnection.Dispose();
        //            }
        //        }
        //        catch (Exception)
        //        {
        //        }
        //        finally
        //        {
        //            _dbConnection = null;
        //        }
        //    }
        //    // Free native resources
        //}

        //public void Dispose()
        //{
        //    Dispose(true);
        //    GC.SuppressFinalize(this);
        //}

        //#endregion IDisposable Members

        ///// <inheritdoc/>
        //~SmartMixDatabaseBase()
        //{
        //    Dispose(false);
        //}
    }
}
