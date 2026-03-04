using System.Runtime.Serialization;

namespace SmartMix.Core.Domain.Entities.Base
{
    /// <summary>
    /// Класс описания базового бизнес-объекта SmartMix.
    /// </summary>
    [Serializable]
    public abstract class BusinessObject
    {
        /// <summary>
        /// Возвращает или задаёт уникальный идентификатор.
        /// </summary>
        [DataMember]
        public int Id { get; set; }

        /// <summary>
        /// Возвращает или задаёт наименование.
        /// </summary>
        [DataMember]
        public string Name { get; set; }
    }
}
