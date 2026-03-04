using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartMix.Core.Common.Data
{
    public interface IObjectSerializer
    {
        /// <summary>
        /// Загружает из файла
        /// </summary>
        /// <typeparam name="T">Класс для десериализации</typeparam>
        /// <param name="fileName">Путь к файлу</param>
        /// <param name="defaultSettings">Стандартный файл из проекта</param>
        /// <param name="log">Логирование</param>
        /// <returns>Десериализованный класс. если файл не найден, сохраняет и возвращает стандартные значения</returns>
        T LoadObject<T>(string fileName, string defaultSettings, Action<string> log) where T : class;

        /// <summary>
        /// Сохраняет настройки в файл
        /// </summary>
        /// <typeparam name="T">Класс для сериализации</typeparam>
        /// <param name="serializableObject">Объект сериализации</param>
        /// <param name="fileName">Путь к файлу</param>
        /// <param name="log">Логирование</param>
        void SaveObject<T>(T serializableObject, string fileName, Action<string> log) where T : class;
    }

    public interface IObjectSerializerProtection
    {
        /// <summary>
        /// Загружает из файла
        /// </summary>
        /// <typeparam name="T">Класс для десериализации</typeparam>
        /// <param name="fileName">Путь к файлу</param>
        /// <param name="defaultSettings">Стандартный файл из проекта</param>
        /// <param name="pass">Пароль шифрования</param>
        /// <param name="log">Логирование</param>
        /// <returns>Десериализованный класс. если файл не найден, сохраняет и возвращает стандартные значения</returns>
        T LoadObjectDecrypted<T>(string fileName, string defaultSettings, string pass, Action<string> log) where T : class;

        /// <summary>
        /// Сохраняет настройки в файл
        /// </summary>
        /// <typeparam name="T">Класс для сериализации</typeparam>
        /// <param name="serializableObject">Объект сериализации</param>
        /// <param name="fileName">Путь к файлу</param>
        /// <param name="pass">Пароль шифрования</param>
        /// <param name="log">Логирование</param>
        void SaveObjectEncrypted<T>(T serializableObject, string fileName, string pass, Action<string> log) where T : class;
    }

    public class ObjectSerializer : IObjectSerializer, IObjectSerializerProtection
    {
        /// <summary>
        /// Загружает из файла
        /// </summary>
        /// <typeparam name="T">Класс для десериализации</typeparam>
        /// <param name="fileName">Путь к файлу</param>
        /// <param name="defaultXML">Стандартный файл из проекта</param>
        /// <returns>Десериализованный класс. если файл не найден, сохраняет и возвращает стандартные значения</returns>
        public T LoadObject<T>(string fileName, string defaultXML, Action<string> log) where T : class // todo дублирование кода с BaseConfiguraion - вынести в статичный класс
        {
            if (!File.Exists(fileName))
                SaveObject(DataSerializer<T>.LoadFromXml(defaultXML), fileName, log);

            T serilizableObj = null;
            try
            {
                serilizableObj = DataSerializer<T>.LoadObject(fileName);
            }
            catch (UnauthorizedAccessException uae)
            {
                //TODO
                log?.Invoke($"{string.Format("file", fileName)} {uae}");
                throw;
            }
            catch (Exception e)
            {
                log?.Invoke($"Возникла непредвиденная ошибка во время загрузки конфигурационного файла {fileName}. {e}");

                File.Delete(fileName);
                try
                {
                    serilizableObj = DataSerializer<T>.LoadObject(fileName);
                }
                catch (Exception ex)
                {
                    log?.Invoke($"Произошла ошибка при загрузке настроек из файла {fileName}: {ex.GetBaseException().Message} Идёт загрузка настроек по умолчанию..");

                    serilizableObj = DataSerializer<T>.LoadFromXml(defaultXML);

                    log?.Invoke("Конфигурация по умолчанию успешно загружена");
                }
            }
            return serilizableObj;
        }

        /// <summary>
        /// Сохраняет настройки в файл
        /// </summary>
        /// <typeparam name="T">Класс для сериализации</typeparam>
        /// <param name="serializableObject">Объект сериализации</param>
        /// <param name="fileName">Путь к файлу</param>
        public void SaveObject<T>(T serializableObject, string fileName, Action<string> log) where T : class
        {
            try
            {
                DataSerializer<T>.SaveObject(serializableObject, fileName);
            }
            catch (DirectoryNotFoundException)
            {
                try
                {
                    DataSerializer<T>.SaveObject(serializableObject, fileName);
                }
                catch (Exception ex)
                {
                    log?.Invoke($"Сбой при сохранении настроек {nameof(T)}: {ex}");
                }
            }
            catch (Exception ex)
            {
                log?.Invoke($"Сбой при сохранении настроек {nameof(T)}: {ex}");
            }
        }

        public void SaveObjectEncrypted<T>(T serializableObject, string fileName, string pass, Action<string> log) where T : class
        {
#if DEBUG
            SaveObject(serializableObject, fileName, log);
#else
            try
            {
                DataSerializer<T>.SaveObjectEncrypted(serializableObject, fileName, pass);
            }
            catch (DirectoryNotFoundException)
            {
                try
                {
                    DataSerializer<T>.SaveObjectEncrypted(serializableObject, fileName, pass);
                }
                catch (Exception ex)
                {
                    log?.Invoke($"Сбой при сохранении настроек {nameof(T)}: {ex}");
                }
            }
            catch (Exception ex)
            {
                log?.Invoke($"Сбой при сохранении настроек {nameof(T)}: {ex}");
            }
#endif
        }

        public T LoadObjectDecrypted<T>(string fileName, string defaultSettings, string pass, Action<string> log) where T : class
        {
#if DEBUG
            if (!File.Exists(fileName))
                SaveObjectEncrypted(DataSerializer<T>.LoadFromXml(defaultSettings), fileName, pass, log);

            return LoadObject<T>(fileName, defaultSettings, log);
#else
            T serilizableObj = null;

            if (!File.Exists(fileName))
                SaveObjectEncrypted(DataSerializer<T>.LoadFromXml(defaultSettings), fileName, pass, log);

            try
            {
                serilizableObj = DataSerializer<T>.LoadObjectDecrypted(fileName, pass);
            }
            catch (Exception)
            {
                File.Delete(fileName);
                try
                {
                    serilizableObj = DataSerializer<T>.LoadObjectDecrypted(fileName, pass);
                }
                catch (Exception ex)
                {
                    log?.Invoke($"Сбой при загрузке настроек {nameof(T)}: {ex.ToString()}");
                    serilizableObj = DataSerializer<T>.LoadFromXml(defaultSettings);
                    log?.Invoke("Стандартные настройки успешно загружены");
                }
            }
            return serilizableObj;
#endif
        }
    }
}
