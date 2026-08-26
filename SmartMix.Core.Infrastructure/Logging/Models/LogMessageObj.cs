namespace TTS.Logger.Interfaces
{
    using System;

    /// <summary>
    /// Представляет класс описания записи лога.
    /// </summary>
    internal class LogMessageObj
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса.
        /// </summary>
        public LogMessageObj()
        {
            DateTime = DateTime.Now;
        }

        /// <summary>
        /// Возвращает или задаёт текст сообщения.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Возвращает или задаёт уровень лога.
        /// </summary>
        public LogLevel Level { get; set; }

        /// <summary>
        /// Возвращает или задаёт дату и время операции.
        /// </summary>
        public DateTime DateTime { get; set; }

        /// <summary>
        /// Возвращает или задаёт источник логирования.
        /// </summary>
        public string Source { get; set; }
    }
}
