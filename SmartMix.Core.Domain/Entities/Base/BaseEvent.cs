using System.Runtime.Serialization;

namespace SmartMix.Core.Domain.Entities.Base
{
    /// <summary>
    /// Представляет класс описания базовой информации по событию.
    /// </summary>
    [Serializable]
    public abstract class BaseEvent
    {
        /// <summary>Дату и время регистрации события.</summary>
        [DataMember]
        public DateTime Date { get; set; }

        /// <summary>
        /// Возвращает или задаёт текущее значение параметра (при наличии смены значения параметра).
        /// </summary>
        [DataMember]
        public string ValueBefore { get; set; }

        /// <summary>
        /// Возвращает или задаёт новое значение параметра (при наличии смены значения параметра).
        /// </summary>
        [DataMember]
        public string ValueAfter { get; set; }

        /// <summary>
        /// Возвращает признак события по изменению значения.
        /// </summary>
        public bool HasValues
        {
            get
            {
                return !string.IsNullOrEmpty(ValueBefore) || !string.IsNullOrEmpty(ValueAfter);
            }
        }
    }
}
