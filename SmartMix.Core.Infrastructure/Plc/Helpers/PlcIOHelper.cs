using System.ComponentModel;
using System.Reflection;

namespace SmartMix.Core.Infrastructure.Plc.Helpers
{
    /// <summary>
    /// Представляет вспомогательный класс по работе с драйвером Modbus TCP.
    /// </summary>
    public static class PlcIOHelper
    {
        public static ushort[] ConvertBytesToUshorts(byte[] data)
        {
            if (data == null) return null;

            ushort[] outdata = new ushort[data.Length / 2];

            for (int i = 0, j = 0; i < data.Length; i += 2, j++)
            {
                outdata[j] = (ushort)((ushort)(data[i] << 8) + data[i + 1]);
            }
            return outdata;
        }

        public static byte[] ConvertUshortToBytes(ushort[] data)
        {
            byte[] outdata = new byte[data.Length * sizeof(ushort)];

            for (int i = 0, j = 0; i < data.Length; i++, j += 2)
            {
                outdata[j + 1] = (byte)(data[i] & 0xff);
                outdata[j] = (byte)(data[i] >> 8);
            }
            return outdata;
        }

        /// <summary>
        /// Возвращает словарь для указанного перечисления, где ключом является значение перечисления, а значение - строковое описание из атрибута <see cref="DescriptionAttribute"/>.
        /// </summary>
        /// <typeparam name="TEnum">Тип перечисления.</typeparam>
        /// <returns></returns>
        internal static Dictionary<TEnum, string> GetEnumValues<TEnum>()
            where TEnum : struct, IConvertible
        {
            if (!typeof(TEnum).IsEnum)
                throw new ArgumentException("Неверный тип");

            Dictionary<TEnum, string> dict = new Dictionary<TEnum, string>();

            Array values = Enum.GetValues(typeof(TEnum));
            foreach (TEnum item in values)
                dict.Add(item, GetEnumDescription(item));

            return dict;
        }

        /// <summary>
        /// Возвращает данные из атрибута <see cref="DescriptionAttribute"/>.
        /// </summary>
        /// <param name="value">Значение перечисления.</param>
        /// <returns>Значение атрибута.</returns>
        internal static string GetEnumDescription<T>(T value)
        {
            var memberInfo = typeof(T).GetMember(value.ToString()).SingleOrDefault();
            if (memberInfo == null) return value.ToString();
            var descriptionAttribute =
                memberInfo.GetCustomAttribute<DescriptionAttribute>();
            return descriptionAttribute != null ? descriptionAttribute.Description : string.Empty;
        }

        /// <summary>
        /// Возвращает перечисление по значению атрибута <see cref="DescriptionAttribute"/>.
        /// </summary>
        /// <typeparam name="TEnum">Тип перечисления.</typeparam>
        /// <param name="description">Значение атрибута</param>
        /// <param name="compare">Сравнение строк.</param>
        /// <returns>Значение перечисления.</returns>
        /// <exception cref="InvalidEnumArgumentException">Исключение, которое генерируется, если атрибут не был сопоставлен ни с одним значением перечисления.</exception>
        internal static TEnum GetEnum4Description<TEnum>(string description, StringComparison compare = StringComparison.InvariantCultureIgnoreCase)
            where TEnum : struct, IConvertible
        {
            Dictionary<TEnum, string> dict = GetEnumValues<TEnum>();
            foreach (KeyValuePair<TEnum, string> kvp in dict)
            {
                if (string.Equals(kvp.Value, description, compare))
                    return kvp.Key;
            }

            throw new InvalidEnumArgumentException(description);
        }

        /// <summary>
        /// Возвращает перечисление по значению атрибута <see cref="DescriptionAttribute"/>.
        /// </summary>
        /// <typeparam name="TEnum">Тип перечисления.</typeparam>
        /// <param name="description">Значение атрибута</param>
        /// <param name="compare">Сравнение строк.</param>
        /// <param name="separator">Разделитель значений в атрибуте, если с перечислением связано несколько описателей.</param>
        /// <returns>Значение перечисления.</returns>
        /// <exception cref="InvalidEnumArgumentException">Исключение, которое генерируется, если атрибут не был сопоставлен ни с одним значением перечисления.</exception>
        internal static TEnum GetEnum4Description<TEnum>(string description, char separator, StringComparison compare = StringComparison.InvariantCultureIgnoreCase)
            where TEnum : struct, IConvertible
        {
            Dictionary<TEnum, string> dict = GetEnumValues<TEnum>();
            foreach (KeyValuePair<TEnum, string> kvp in dict)
            {
                string[] values = kvp.Value.Split(separator);
                for (int i = 0; i < values.Length; i++)
                {
                    if (string.Equals(values[i], description, compare))
                        return kvp.Key;
                }
            }

            throw new InvalidEnumArgumentException(description);
        }

        internal static string GetFormatDescription(string description)
        {
            return description; // TODO: сделать парсер для описания или сделать правильное описание сетевых на PLC
        }
    }
}
