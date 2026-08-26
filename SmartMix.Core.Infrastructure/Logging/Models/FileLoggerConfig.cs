namespace TTS.Logger.Interfaces
{
    /// <summary>
    /// Конфигурация логирования
    /// </summary>
    public class FileLoggerConfig
    {
        /// <summary> Путь до файла лога </summary>
        public string LogPath { get; set; }

        /// <summary> Название лога </summary>
        public string LogName { get; set; }

        /// <summary>Возвращает или задаёт минимальный уровень сообщений лога</summary>
        public LogLevel LogLevel { get; set; }
    }
}