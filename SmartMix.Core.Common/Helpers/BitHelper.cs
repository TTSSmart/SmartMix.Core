namespace SmartMix.Core.Common.Helpers
{
    public static class BitHelper
    {
        /// <summary>
        /// Получить bool значение из переданного в метод числа по указанному номеру бита
        /// </summary>
        /// <param name="value">Число, из которого нужно получить значение указанного бита</param>
        /// <param name="bitNumber">Номер бита</param>
        /// <returns></returns>
        public static bool GetBoolFromBitValue(uint value, int bitNumber)
        {
            return ((value & (uint)(1 << (bitNumber - 1))) > 0);
        }

        /// <summary>
        /// В переданное число в указанный номер бита записать значение 1 или 0, то есть 3-ий параметр, который если true,
        /// то пишется 1, если false, то пишется 0
        /// </summary>
        /// <param name="value">Число, из которого нужно получить значение указанного бита</param>
        /// <param name="bitNumber">Номер бита</param>
        /// <param name="newValue">Значение, которое будет записано по указанному номеру бита. true - 1, false - 0</param>
        /// <returns></returns>
        public static uint WriteBoolToBitValue(uint value, int bitNumber, bool newValue)
        {
            uint mask = (uint)(1 << (bitNumber - 1));
            if (newValue)
            {
                return value | mask;
            }
            return value & ~mask;
        }
    }
}
