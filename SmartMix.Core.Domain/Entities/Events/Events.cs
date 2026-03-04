using SmartMix.Core.Domain.Entities.Base;
using SmartMix.Core.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SmartMix.Core.Domain.Entities.Events
{
    /// <summary>
    /// Представляет класс описания записи из журнала событий в БД.
    /// </summary>
    [DataContract]
    public class Event : BaseEvent
    {
        /// <summary>
        /// Возвращает или задаёт статус [аварийного] события.
        /// </summary>
        [DataMember]
        public EventStatus Status { get; set; }

        /// <summary>
        /// Возвращает или задаёт уникальное название объекта.
        /// </summary>
        /// <remarks>см. таблицу event_objects</remarks>
        [DataMember(IsRequired = true)]
        public string Object { get; set; }

        /// <summary>
        /// Возвращает или задаёт уникальный идентификатор типа события eventType.Id
        /// </summary>
        [DataMember(IsRequired = true)]
        public int TypeId { get; set; }

        /// <summary>
        /// Возвращает или задаёт ID пользователя.
        /// </summary>
        [DataMember(IsRequired = true)]
        public int UserId { get; set; }
    }
}
