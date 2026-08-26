using SmartMix.Core.Infrastructure.Persistence.Database.Models;
using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;

namespace SmartMix.Core.Infrastructure.Persistence.Database.Interface
{
    /// <summary>
    /// Представляет интерфейс БД SmartMix.
    /// </summary>
    public interface ISmartMixDatabase
    {
        Task<DbOperationResult<bool>> CheckConnectionAsync(CancellationToken cancellationToken);
        DbOperationResult<List<DbDataRecord>> GetData(DbCommand command);
        Task<DbOperationResult<List<DbDataRecord>>> GetDataAsync(DbCommand command, CancellationToken cancellationToken);
        Task<DbOperationResult<IList<T>>> GetDataAsync<T>(DbCommand command, Func<DbDataReader, IList<T>> converterFunc, CancellationToken cancellationToken);
        Task<DbOperationResult<T>> GetDataAsync<T>(DbCommand command, Func<DbDataReader, T> converterFunc, CancellationToken cancellationToken);
        Task<DbOperationResult<T>> GetDataFirstAsync<T>(DbCommand command, Func<DbDataReader, IList<T>> converterFunc, CancellationToken cancellationToken, [CallerMemberName] string memberName = "");
        Task<DbOperationResult<object>> ExecuteQueryAsync(DbCommand command, CancellationToken cancellationToken);
        Task<DbOperationResult<DataTable>> GetDataTableAsync(DbDataAdapter dataAdapter, CancellationToken cancellationToken);

        Task<DbOperationResult<int>> ChangeDataAsync(DbCommand command, CancellationToken cancellationToken);
        Task<DbOperationResult<bool>> ChangeDataAsync(DbCommand[] command, CancellationToken cancellationToken);
        Task<DbOperationResult<Dictionary<Guid, int>>> ChangeDataAsync(Dictionary<Guid, DbCommand> command, CancellationToken cancellationToken);

        DbCommand CreateCommand(string commandText);

        /// <summary>
        /// Возвращает результат выполнения хранимой процедуры как кеш данных в памяти типа DataSet.
        /// </summary>
        /// <param name="procedureName">Наименование хранимой процедуры.</param>
        /// <param name="parameters">Массив значений параметров хранимой процедуры.</param>
        /// <returns>Результат выполнения ХП типа DataSet.</returns>
        /// <remarks>Функция виртуальная только из-за тестов.</remarks>
        DataSet ExecuteProcedure(string procedureName, DbParameter[] parameters);

        /// <summary>
        /// Возвращает результат выполнения команды как кеш данных в памяти типа DataSet.
        /// </summary>
        /// <param name="cmdText">Текст исполняемой команды.</param>
        /// <param name="parameters">Массив значений параметров запроса.</param>
        /// <returns>Результат выполнения команды типа DataSet.</returns>
        DataSet ExecuteCommand(string cmdText, params DbParameter[] parameters);

        /// <summary>
        /// Выполняет команду без возврата значений.
        /// </summary>
        /// <param name="cmdText">Текст исполняемой команды.</param>
        /// <param name="parameters">Массив значений параметров запроса.</param>
        void ExecuteNonQuery(string cmdText, params DbParameter[] parameters);

        /// <summary>
        /// Выполняет команду, возвращающую значение указанного типа.
        /// </summary>
        /// <param name="cmdText">Текст команды.</param>
        /// <param name="parameters">Массив значений входных параметров.</param>
        /// <returns>Значение указанного типа из первого столбца первой строки результирующего набора данных.</returns>
        T ExecuteScalar<T>(string cmdText, params DbParameter[] parameters);
    }
}
