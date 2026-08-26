using System.Collections.Concurrent;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using TTS.Logger.Interfaces;

namespace TTS.Logger
{
    /// <summary>
    /// Менеджер протоколирования работы системы.
    /// </summary>
    public class LogManager
    {
        #region Singleton
        private static readonly Lazy<LogManager> _lazy = new Lazy<LogManager>(() => new LogManager());
        /// <summary> Возвращает единственный экземпляр объекта в памяти.</summary>
        public static LogManager Instance => _lazy.Value;

        #endregion

        /// <summary>
        /// Представляет очередь сообщений записи в лог.
        /// </summary>
        private readonly BlockingCollection<LogMessageObj> _queue = new BlockingCollection<LogMessageObj>(new ConcurrentQueue<LogMessageObj>(), 5000);
        private readonly Task _task;

        /// <summary>
        /// Представляет список текущих логеров.
        /// </summary>
        private readonly List<ILogger> _loggers = new List<ILogger>();

        /// <summary>
        /// Представляет минимальный уровень логирования.
        /// </summary>
        private LogLevel _minLevel = LogLevel.Info;

        /// <summary>
        /// Инициализирует новый экземпляр класса.
        /// </summary>
        public LogManager()
        {
            _task = Task.Run(Consumer);
        }

        /// <summary>
        /// Добавляет логер с фиксацией минимального уровня логирования.
        /// </summary>
        /// <param name="logger">Логер.</param>
        public void AddLogger(ILogger logger)
        {
            if (_minLevel > logger.Level)
                _minLevel = logger.Level;

            _loggers.Add(logger);
        }

        /// <summary>
        /// Фиксирует сообщение с указанными параметрами.
        /// </summary>
        /// <param name="message">Сообщение.</param>
        /// <param name="logLevel">Уровень логирования.</param>
        /// <param name="logSource">Источник лога.</param>
        public void Write(string message, LogLevel logLevel, [CallerMemberName] string logSource = "")
        {
            if (logLevel >= _minLevel)
                _queue.TryAdd(new LogMessageObj { Message = message, Level = logLevel, Source = logSource });
        }

        /// <summary>
        /// Фиксирует отладочное сообщение с указанным текстом.
        /// </summary>
        /// <param name="message">Сообщение.</param>
        /// <param name="logSource">Источник логирования.</param>
        public void WriteDebug(string message, [CallerMemberName] string logSource = "") => Write(message, LogLevel.Debug, logSource);

        /// <summary>
        /// Фиксирует информационное сообщение с указанным текстом.
        /// </summary>
        /// <param name="message">Сообщение.</param>
        /// <param name="logSource">Источник логирования.</param>
        public void WriteInfo(string message, [CallerMemberName] string logSource = "") => Write(message, LogLevel.Info, logSource);

        /// <summary>
        /// Фиксирует предупреждение с указанным текстом.
        /// </summary>
        /// <param name="message">Сообщение.</param>
        /// <param name="logSource">Источник логирования.</param>
        public void WriteWarning(string message, [CallerMemberName] string logSource = "") => Write(message, LogLevel.Warning, logSource);

        /// <summary>
        /// Фиксирует предупреждение c указанным текстом для исключения.
        /// </summary>
        /// <param name="message">Сообщение.</param>
        /// <param name="ex">Исключение</param>
        /// <param name="logSource">Источник логирования.</param>
        public void WriteWarning(string message, Exception ex, [CallerMemberName] string logSource = "") => Write($"{message} {ex}", LogLevel.Warning, logSource);

        /// <summary>
        /// Фиксирует предупреждение для указанного исключения.
        /// </summary>
        /// <param name="ex">Исключение</param>
        /// <param name="logSource">Источник логирования.</param>
        public void WriteWarning(Exception ex, [CallerMemberName] string logSource = "") => Write(ex.ToString(), LogLevel.Warning, logSource);

        /// <summary>
        /// Фиксирует ошибку с указанным текстом.
        /// </summary>
        /// <param name="message">Сообщение.</param>
        /// <param name="logSource">Источник логирования.</param>
        public void WriteError(string message, [CallerMemberName] string logSource = "") => Write(message, LogLevel.Error, logSource);

        /// <summary>
        /// Фиксирует ошибку c указанным текстом для исключения.
        /// </summary>
        /// <param name="message">Сообщение.</param>
        /// <param name="ex">Исключение</param>
        /// <param name="logSource">Источник логирования.</param>
        public void WriteError(string message, Exception ex, [CallerMemberName] string logSource = "") => Write($"{message} {ex}", LogLevel.Error, logSource);

        public void WriteExtended(string message, LogLevel logLevel, [CallerMemberName] string logSource = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            Write(message, logLevel, $"{logSource}[{sourceLineNumber}]");
        }

        /// <summary>
        /// Фиксирует сообщение об ошибке для указанного исключения.
        /// </summary>
        /// <param name="ex">Исключение</param>
        public void WriteException(Exception ex, [CallerMemberName] string logSource = "") => Write(ex.ToString(), LogLevel.Error, logSource);

        public async Task Flush()
        {
            _queue.CompleteAdding();
            await _task.ConfigureAwait(false);

            foreach (ILogger item in _loggers)
            {
                if (item is IDisposable idisp)
                    idisp.Dispose();
            }
        }

        private async Task Consumer()
        {
            foreach (LogMessageObj logItem in _queue.GetConsumingEnumerable())
            {
                foreach (ILogger item in _loggers)
                {
                    if (logItem.Level >= item.Level)
                        await item.LogMessage($"{logItem.DateTime.ToString("dd.MM.yyyy HH:mm:ss.ffff")} [{GetEnumDescription(logItem.Level),-5}] {logItem.Source,-25} {logItem.Message}", logItem.Level);
                }
            }
        }

        /// <summary>
        /// Возвращает данные из атрибута <see cref="DescriptionAttribute"/>.
        /// </summary>
        /// <param name="value">Значение перечисления.</param>
        /// <returns>Значение атрибута.</returns>
        private static string GetEnumDescription<T>(T value)
        {
            var memberInfo = typeof(T).GetMember(value.ToString()).SingleOrDefault();
            if (memberInfo == null) return value.ToString();
            var descriptionAttribute =
                memberInfo.GetCustomAttribute<DescriptionAttribute>();
            return descriptionAttribute != null ? descriptionAttribute.Description : string.Empty;
        }
    }
}
