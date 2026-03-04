using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartMix.Core.Common.Extentions
{
    public static class DataExtension
    {
        /// <summary>
        /// Конец дня
        /// </summary>
        /// <param name="date">Время</param>
        /// <returns></returns>
        public static DateTime ToEndOfDay(this DateTime date)
            => date.AddDays(1).AddTicks(-1);

        /// <summary>
        /// Конец месяца 
        /// </summary>
        /// <param name="date">Время</param>
        /// <returns></returns>
        public static DateTime ToLastDayOfMonth(this DateTime date)
            => date.AddDays(1 - (date.Day)).AddMonths(1).AddTicks(-1);

        /// <summary>
        /// Получить разницу с текущей датой в миллисекундах
        /// </summary>
        /// <param name="date">Дата</param>
        /// <returns></returns>
        public static double DifferenceToMilliseconds(this DateTime date)
        {
            var actualTime = DateTime.Now;
            if (actualTime.TimeOfDay >= date.TimeOfDay)
            {
                actualTime = actualTime.AddDays(1);
            }
            actualTime = actualTime.AddHours(0 - actualTime.Hour + date.Hour);
            actualTime = actualTime.AddMinutes(0 - actualTime.Minute + date.Minute);
            actualTime = actualTime.AddSeconds(0 - actualTime.Second);

            return Math.Abs((actualTime - DateTime.Now).TotalMilliseconds);
        }
    }
}
