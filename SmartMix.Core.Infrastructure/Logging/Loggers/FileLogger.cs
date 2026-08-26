using System;
using System.IO;
using System.Threading.Tasks;
using TTS.Logger.Interfaces;
using TTS.Logger.Models;

namespace TTS.Logger.Loggers
{
    public class FileLogger : ILogger, IDisposable
    {
        private readonly RWLock _rwLock = new RWLock();

        private readonly System.Threading.Timer _flushTimer;
        private readonly System.Threading.Timer _flushDayTimer;

        /// <summary>
        /// Представляет рапер лога.
        /// </summary>
        private StreamWriter _sw;

        /// <summary>
        /// Представляет конфигурацию файлового логера.
        /// </summary>
        private readonly FileLoggerConfig _logConfig;

        /// <summary>
        /// Представляет дату текущего лога.
        /// </summary>
        private static DateTime _curDate = DateTime.Today;

        /// <summary>
        /// Инициализирует новый экземпляр класса с указанной конфигурацией.
        /// </summary>
        /// <param name="logConfig">Конфигурация файлового логера.</param>
        /// <exception cref="ArgumentNullException">Исключение, которое генерируется, если конфигурация не была определена.</exception>
        public FileLogger(FileLoggerConfig logConfig)
        {
            _logConfig = logConfig ?? throw new ArgumentNullException(nameof(logConfig));

            if (!Directory.Exists(logConfig.LogPath))
                Directory.CreateDirectory(logConfig.LogPath);

            OpenStream();

            // Сброс логов в файл. Первый раз через 15 секунд, после - каждую минуту.
            _flushTimer = new System.Threading.Timer(WriteLog, null, TimeSpan.FromSeconds(15), TimeSpan.FromMinutes(1));
            _flushDayTimer = new System.Threading.Timer(CompressDayLog, null, TimeSpan.FromHours(1), TimeSpan.FromHours(24));
        }

        /// <summary>
        /// Возвращает минимальный уровень логирования.
        /// </summary>
        public LogLevel Level
        {
            get
            {
                return _logConfig?.LogLevel ?? LogLevel.Info;
            }
        }

        /// <summary>
        /// Добавляет запись лога  с указанным сообщением и уровнем сообщения.
        /// </summary>
        /// <param name="message">Отформатированная строка лога.</param>
        /// <param name="curLevelMessage">Уровень сообщения.</param>
        /// <returns></returns>
        public async Task LogMessage(string message, LogLevel curLevelMessage)
        {
            await _sw.WriteLineAsync(message);
        }

        /// <summary>
        /// Выполняет сброс сообщений из буфера обмена
        /// </summary>
        /// <param name="obj"></param>
        private void WriteLog(object obj)
        {
            if (DateTime.Today > _curDate.Date)
            {
                // смена дня
                OpenStream();
            }
            else
            {
                using (_rwLock.WriteLock())
                {
                    _sw.Flush();
                }
            }
        }

        /// <summary>
        /// Выполняет переинициализацию рапера.
        /// Возвращает наименование текущего файла лога.
        /// </summary>
        /// <returns>Абсолютный путь к текущему файлу лога.</returns>
        private string OpenStream()
        {
            if (_sw != null)
            {
                using (_rwLock.WriteLock())
                {
                    _sw.Flush();
                    _sw.Close();
                    _sw.Dispose();
                }
            }

            string filePath = GetFullPathName(_logConfig);
            _sw = new StreamWriter(filePath, true, System.Text.Encoding.UTF8);

            return filePath;
        }

        /// <summary>
        /// Возвращает полный путь к текущему файлу лога.
        /// </summary>
        /// <param name="logConfg">Конфигурация логера.</param>
        /// <returns>Абсолютный путь к файлу.</returns>
        private static string GetFullPathName(FileLoggerConfig logConfg)
        {
            _curDate = DateTime.Now; // сменили
            return Path.Combine(logConfg.LogPath, $"{logConfg.LogName}_{_curDate.ToString("yyyy-MM-dd HH.mm")}.txt");
        }

        private void CompressDayLog(object obj)
        {
            string currentLog = OpenStream();

            FileOperations.CompressAllFiles(Directory.GetFiles(_logConfig.LogPath, "*.txt"), currentLog);
        }

        #region IDisposable Members

        private bool _disposed;

        /// <summary>
        /// Разрывает соединение с контроллером ПЛК.
        /// </summary>
        /// <param name="disposing">Значение <see langword="true"/>, если метод вызывается из Dispose(), значение <see langword="false"/>, если метод вызывается из метода завершения.</param>
        private void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                // Dispose managed resources
                try
                {
                    _sw?.Close();
                    _flushTimer?.Dispose();
                    _rwLock?.Dispose();

                    _flushDayTimer?.Dispose();
                }
                catch (Exception)
                {
                    // ignore
                }
            }

            // Free native resources
            _disposed = true;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Members

        /// <inheritdoc/>
        ~FileLogger()
        {
            Dispose(false);
        }
    }
}