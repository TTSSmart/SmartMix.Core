using SmartMix.Core.Domain.Enums;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace SmartMix.Core.Domain.Entities.Events
{
    /// <summary>
    /// Представляет класс описания типа события.
    /// </summary>
    /// <remarks>см. таблицу event_types</remarks>
    [DebuggerDisplay("Id = {Id}; EventLevel = {Level}; Name = {Description}; ")]
    [DataContract]
    public class EventType
    {
        /// <summary>Уникальный код события</summary>
        [DataMember]
        public int Id { get; set; }

        /// <summary>Уровень события</summary>
        [DataMember]
        public EventLevel Level { get; set; }

        /// <summary>Название типа события</summary>
        [DataMember]
        public string Description { get; set; }

        /// <summary>Детали события</summary>
        /// <example>Информация, Авария, Предупреждение, Изменена настройка, Изменена команда</example>
        [DataMember]
        public string Details { get; set; }

        /// <summary>
        /// Выполняет создание объекта типа <see cref="EventType"/> с указанным уровнем события.
        /// </summary>
        /// <param name="kvp">Пара значений, где ключом является уровень события, а значением - текстовое описание уровня события.</param>
        /// <returns>Тип события.</returns>
        public static EventType ConvertFrom(KeyValuePair<EventLevel, string> kvp) // todo подключить
        {
            return new EventType()
            {
                Level = kvp.Key,
                Description = kvp.Value
            };
        }
    }
}
