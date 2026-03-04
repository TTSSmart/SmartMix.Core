using SmartMix.Core.Domain.Entities.Base.Shared;
using SmartMix.Core.Domain.Enums.Consistences;
using System.Runtime.Serialization;

namespace SmartMix.Core.Domain.Entities.Consistences
{
    /// <summary>Шапка таблицы консистенции</summary>
    [DataContract]
    public class HeaderConsistency : IEquatable<HeaderConsistency>, ICloneable<HeaderConsistency>
    {
        /// <summary>Уникальный идентификатор/в базе данных</summary>
        [DataMember(IsRequired = true)]
        public int Id { get; set; } = -1;

        /// <summary>Название консистенции</summary>
        [DataMember(IsRequired = true)]
        public string Name { get; set; } = string.Empty;

        /// <summary>Возвращает или задаёт отображаемый вид консистенции (конус или марка)</summary>
        [DataMember(IsRequired = true)]
        public ConsistencyDisplay ConsistencyDisplay { get; set; }

        #region IEquatable

        /// <summary>Возвращает значение, указывающее, равен ли этот экземпляр заданному значению типа HeaderConsistency</summary>
        /// <param name="other"> Значение типа HeaderConsistency для сравнения с данным экземпляром</param>
        /// <returns>true, если значение параметра other совпадает со значением данного экземпляра;в противном случае — false</returns>
        public bool Equals(HeaderConsistency other)
        {
            if (other == null) return false;

            return (string.Compare(Name, other.Name, StringComparison.CurrentCulture) == 0
                        && ConsistencyDisplay == other.ConsistencyDisplay
                        && Id == other.Id);
        }

        /// <summary>
        /// Сравнение 
        /// </summary>
        /// <param name="other">Объект с которым необходимо сравнить</param>
        /// <returns></returns>
        public override bool Equals(object other)
        {
            if (other == null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (GetType() != other.GetType())
                return false;

            return Equals((HeaderConsistency)other);
        }

        /// <summary>
        /// хэш-код класса
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Id ^ Name.GetHashCode() ^ ConsistencyDisplay.GetHashCode();
        }

        public static bool operator ==(HeaderConsistency person1, HeaderConsistency person2)
        {
            if (((object)person1) == null || ((object)person2) == null)
                return Object.Equals(person1, person2);

            return person1.Equals(person2);
        }

        public static bool operator !=(HeaderConsistency person1, HeaderConsistency person2)
        {
            if (((object)person1) == null || ((object)person2) == null)
                return !Object.Equals(person1, person2);

            return !(person1.Equals(person2));
        }
        #endregion

        #region ICloneable

        /// <summary> 
        /// Копирование</summary>
        /// <returns></returns>
        public HeaderConsistency Clone()
        {
            return (HeaderConsistency)MemberwiseClone();
        }

        #endregion ICloneable

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{nameof(Name)} = {Name}, {nameof(Id)} = {Id}, {nameof(ConsistencyDisplay)} = {ConsistencyDisplay}";
        }
    }
}
