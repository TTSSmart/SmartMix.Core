namespace SmartMix.Core.Infrastructure.Persistence.Database.Models
{
    /// <summary>
    /// Представляет структуру результата выполнения запроса к БД.
    /// </summary>
    /// <typeparam name="T">Тип выходных данных.</typeparam>
    public struct DbOperationResult<T>
    {
        /// <summary>Инициализирует новый экземпляр класса с указанным текстом ошибки.</summary>
        /// <param name="error">Текст ошибки</param>
        private DbOperationResult(string error)
        {
            Result = default(T);
            Error = error ?? string.Empty;
            Exception = null;

            Success = string.IsNullOrWhiteSpace(error);
        }

        /// <summary>Инициализирует новый экземпляр класса с указанными выходными данными.</summary>
        /// <param name="result">Результат выполнения запроса к БД.</param>
        public DbOperationResult(T result)
        {
            Result = result;
            Error = string.Empty;
            Exception = null;

            Success = string.IsNullOrWhiteSpace(Error);
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса для указанного исключения.
        /// </summary>
        /// <param name="e">Исключение.</param>
        public DbOperationResult(Exception e) : this(e.GetBaseException().Message)
        {
            Exception = e;
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса с указанным текстом <paramref name="error"/> для исключения <paramref name="error"/>.
        /// </summary>
        /// <param name="error">Текст ошибки.</param>
        /// <param name="e">Исключение.</param>
        public DbOperationResult(string error, Exception e) : this(error)
        {
            Exception = e;
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса с указанными выходными данными и исключением.
        /// </summary>
        /// <param name="result">Результат выполнения запроса к БД.</param>
        /// <param name="e">Исключение.</param>
        public DbOperationResult(T result, Exception e) : this(e)
        {
            Result = result;
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса с указанными выходными данными, текстом ошибки и исключением.
        /// </summary>
        /// <param name="result">Результат выполнения запроса к БД.</param>
        /// <param name="error">Текст ошибки.</param>
        /// <param name="e">Исключение.</param>
        public DbOperationResult(T result, string error, Exception e) : this(result, e)
        {
            Error = error;
        }

        /// <summary>
        /// Результат выполнения операции, или выходные данные.
        /// </summary>
        /// <value>Для коллекций в случае ошибок лучше создавать пустые коллекции</value>
        public T Result { get; private set; }

        /// <summary>
        /// Возвращает или задаёт признак успешного выполнения операции.
        /// </summary>
        /// <value>Значение <see langword="true"/>, если операция была выполнена успешно, иначе - значение <see langword="false"/>.</value>
        public bool Success { get; private set; }

        /// <summary>
        /// Возвращает или задаёт краткий текст ошибки.
        /// </summary>
        public string Error { get; private set; }

        /// <summary>
        /// Возвращает или задаёт исключение, которое возникло в процессе выполнения запроса к БД.
        /// </summary>
        public Exception Exception { get; set; }
    }
}
