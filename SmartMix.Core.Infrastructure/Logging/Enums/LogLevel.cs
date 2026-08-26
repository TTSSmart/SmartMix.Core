namespace TTS.Logger.Interfaces
{
    using System.ComponentModel;

    /// <summary>
    /// Уровень сообщения лога
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// Все
        /// </summary>
        [Description("TRACE")]
        All,

        /// <summary>
        /// Debug
        /// </summary>
        [Description("DEBUG")]
        Debug,

        /// <summary>
        /// Информация
        /// </summary>
        [Description("INFO")]
        Info,

        /// <summary>
        /// Предупреждение
        /// </summary>
        [Description("WARN")]
        Warning,

        /// <summary>
        /// Ошибка
        /// </summary>
        [Description("ERROR")]
        Error
    }
}
