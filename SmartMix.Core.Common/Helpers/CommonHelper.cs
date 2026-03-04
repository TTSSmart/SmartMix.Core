namespace SmartMix.Core.Common.Helpers
{
    /// <summary>
    /// Представляет вспомогательный класс по общим методам и функциям.
    /// </summary>
    public static class CommonHelper
    {
        /// <summary>
        /// Рассчитывает количество необходимых замесов для указанного объёма заявки <paramref name="volumeF"/>.
        /// Возвращает результат выполнения операции.
        /// </summary>
        /// <param name="volumeF">Планируемый объем</param>
        /// <param name="mixerVolumeF">Объем смесителя</param>
        /// <returns>Расчётное количество замесов.</returns>
        public static int GetBatchCount(float volumeF, float mixerVolumeF)
        {
            decimal volume = Convert.ToDecimal(volumeF);
            decimal mixerVolume = Convert.ToDecimal(mixerVolumeF);

            if (mixerVolume == 0) return 0;
            if (volume <= mixerVolume) return 1; // для ручного замеса объём = 0

            int count = (int)(volume / mixerVolume);
            if (Math.Abs(volume % mixerVolume) > 0.0001m) count++;

            return count;
        }

        /// <summary>
        /// Рассчитывает объем одного замеса по указанным параметрам.
        /// Возвращает результат выполнения операции.
        /// </summary>
        /// <param name="volumeF">Планируемый объем</param>
        /// <param name="batchCount">Общее количество замесов</param>
        /// <returns>Расчётный объем одного замеса.</returns>
        public static float GetBatchVolume(float volumeF, int batchCount)
        {
            decimal volume = Convert.ToDecimal(volumeF);
            return Convert.ToSingle(Math.Round(volume / batchCount, 3));
        }

        /// <summary>
        /// Выполняет склонение указанного значения <paramref name="value"/>  в днях (винительный падеж). Возвращает результат склонения.
        /// </summary>
        /// <param name="value">Значение.</param>
        /// <returns>Склонение слова в винительном падеже.</returns>
        /// <example>5 дней</example>
        public static string FormatDayString(int value)
        {
            string[] data = new[] { "день", "дня", "дней" };

            long r = value % 100;
            if (r > 19)
                r = value % 10;

            if (r == 1 && r != 11)
                return $"{value} {data[0]}";

            if (r > 1 && r < 5 && r != 12 && r != 13 && r != 14) // 2,3,4
                return $"{value} {data[1]}";

            return $"{value} {data[2]}";
        }

        /// <summary>
        /// Проверяет входное значение <paramref name="value"/> на корректность.
        /// Возвращает обработанное значение.
        /// </summary>
        /// <param name="value">Входное значение.</param>
        /// <param name="defaultValue">Значение по умолчанию, которое будет использоваться, если значение не является числом.</param>
        /// <returns>Значение параметра, если значение является числом, иначе - значение <paramref name="defaultValue"/>.</returns>
        public static float CheckValue(float value, float defaultValue = 0)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                return defaultValue;
            return value;
        }
    }
}
