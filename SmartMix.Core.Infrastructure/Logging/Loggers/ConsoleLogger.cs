using TTS.Logger.Interfaces;

namespace TTS.Logger.Loggers
{
    public class ConsoleLogger : ILogger
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса с минимальным уровнем логирования.
        /// </summary>
        /// <param name="level">Минимальный уровень логирования.</param>
        public ConsoleLogger(LogLevel level)
        {
            Level = level;
        }

        /// <summary>
        /// Возвращает или задаём минимальный уровень логирования.
        /// </summary>
        public LogLevel Level { get; protected set; }

        /// <summary>
        /// Добавляет запись лога  с указанным сообщением и уровнем сообщения.
        /// </summary>
        /// <param name="message">Отформатированная строка лога.</param>
        /// <param name="curLevelMessage">Уровень сообщения.</param>
        /// <returns></returns>
        public virtual async Task LogMessage(string message, LogLevel curLevelMessage) => await Task.Run(() => Console.WriteLine(message));
    }

    public class ColoredConsoleLogger : ConsoleLogger
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса с минимальным уровнем логирования.
        /// </summary>
        /// <param name="level">Минимальный уровень логирования.</param>
        public ColoredConsoleLogger(LogLevel level)
            : base(level)
        {
        }

        /// <summary>
        /// Добавляет запись лога  с указанным сообщением и уровнем сообщения.
        /// </summary>
        /// <param name="message">Отформатированная строка лога.</param>
        /// <param name="curLevelMessage">Уровень сообщения.</param>
        /// <returns></returns>
        public async override Task LogMessage(string message, LogLevel curLevelMessage)
        {
            await Task.Run(() =>
            {
                switch (curLevelMessage)
                {
                    case LogLevel.All:
                    case LogLevel.Debug:
                        WriteColoredLine(ConsoleColor.White, message, true);
                        break;

                    case LogLevel.Info:
                        WriteColoredLine(ConsoleColor.Green, message, true);
                        break;

                    case LogLevel.Warning:
                        WriteColoredLine(ConsoleColor.Yellow, message, true);
                        break;

                    case LogLevel.Error:
                        WriteColoredLine(ConsoleColor.Red, message, true);
                        break;

                    default:
                        break;
                }
            });
        }

        private static void WriteColoredLine(ConsoleColor textColor, string text, bool linebreak = true, params object[] args)
        {
            ConsoleColor currentTextColor = Console.ForegroundColor;
            Console.ForegroundColor = textColor;
            if (linebreak)
                Console.WriteLine(text, args);
            else
                Console.Write(text, args);
            Console.ForegroundColor = currentTextColor;
        }
    }
}