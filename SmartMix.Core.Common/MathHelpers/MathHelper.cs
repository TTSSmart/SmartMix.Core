using System.Globalization;

namespace SmartMix.Core.Common.MathHelpers
{
    public class MathHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="discrete">Дискрет веса.</param>
        /// <param name="inValue">Входное значение.</param>
        /// <param name="maxWeight">Максимальный вес.</param>
        /// <param name="inPercent">Признак подсчета в процентах.</param>
        /// <returns>Отформатированная строка.</returns>
        public static string ConvertToDiscreteFormattedText(double discrete, double inValue, double maxWeight = -1d, bool inPercent = true)
        {
            discrete = discrete > double.Epsilon ? discrete : 1;

            double value = ((int)Math.Round(inValue / discrete)) * discrete;
            int mod = Math.Round(discrete % 1, 2).ToString(CultureInfo.InvariantCulture).Length - 2;
            if (mod < 0) mod = 0;

            if (inPercent)
                return value.ToString("N" + mod);

            double maxW = maxWeight > double.Epsilon ? maxWeight : inValue;
            return inValue != .0d ? (inValue / maxW).ToString("P" + mod) : "-----";

        }
    }
}
