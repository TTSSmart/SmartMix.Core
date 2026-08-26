namespace TTS.Logger.Interfaces
{
    using System.Threading.Tasks;

    public interface ILogger
    {
        /// <summary>
        /// Возвращает минимальный уровень логирования.
        /// </summary>
        LogLevel Level { get; }

        /// <summary>
        /// Добавляет запись лога  с указанным текстом и уровнем сообщения.
        /// </summary>
        /// <param name="message">Отформатированная строка лога.</param>
        /// <param name="curLevelMessage">Уровень сообщения.</param>
        /// <returns></returns>
        Task LogMessage(string message, LogLevel curLevelMessage);
    }
}