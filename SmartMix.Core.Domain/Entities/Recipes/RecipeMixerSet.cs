using SmartMix.Core.Domain.Entities.Base;
using SmartMix.Core.Domain.Entities.Base.Shared;
using SmartMix.Core.Domain.Enums;
using System.Runtime.Serialization;

namespace SmartMix.Core.Domain.Entities.Recipes
{
    /// <summary>Набор импульсов на выгрузку и на задержку</summary>
    [DataContract]
    public class RecipeMixerSet : BaseInfo, ICloneable<RecipeMixerSet>, IEquatable<RecipeMixerSet>
    {
        /// <summary>Инициализирует новый экземпляр класса.</summary>
        public RecipeMixerSet() : base()
        {
            Guid = Guid.Empty;

            UploadMode = MixerUploadType.Constant;
            Delay50 = 3000;
            Delay75 = 3000;
            ImpulseTime = 50;
            TimeBetweenImpulse = 3000;
            ImpulseCount = 0;
        }

        /// <summary>
        /// Возвращает или задаёт признак, привязан ли текущий набор хотя бы к одному рецепту.
        /// Это вычислимое свойство.
        /// </summary>

        [DataMember(IsRequired = true)]
        public bool IsUsed { get; set; }

        /// <summary>Время выгрузки</summary>
        [DataMember(IsRequired = true)]
        public uint TimeDischarge { get; set; }

        /// <summary>время выгрузки на последнем замесе</summary>
        [DataMember(IsRequired = true)]
        public uint TimeExtraUnload { get; set; }

        /// <summary>Режим выгрузки из смесителя</summary>
        [DataMember(IsRequired = true)]
        public MixerUploadType UploadMode { get; set; }

        /// <summary>Количество импульсов</summary>
        [DataMember(IsRequired = true)]
        public uint ImpulseCount { get; set; }

        /// <summary>Время импульса</summary>
        [DataMember(IsRequired = true)]
        public uint ImpulseTime { get; set; }

        /// <summary>Задержка между импульсами</summary>
        [DataMember(IsRequired = true)]
        public uint TimeBetweenImpulse { get; set; }

        /// <summary>Не закрывать затвор после импульса</summary>
        [DataMember(IsRequired = true)]
        public bool NotClose { get; set; }

        /// <summary>Задержка на концевике 1/2</summary>
        [DataMember(IsRequired = true)]
        public uint Delay50 { get; set; }

        /// <summary>Задержка на концевике 3/4</summary>
        [DataMember(IsRequired = true)]
        public uint Delay75 { get; set; }

        /// <summary>
        /// GUID как внешний идентификатор
        /// </summary>
        /// <value>Значение по умолчанию: <see cref="Guid.Empty"/>.</value>
        /// <remarks>Используется при интеграции с 1С</remarks>
        [DataMember(IsRequired = true)]
        public Guid Guid { get; set; }


        /// <inheritdoc/>
        public new RecipeMixerSet Clone()
            => (RecipeMixerSet)MemberwiseClone();


        #region IEquatable
        /// <summary>Возвращает значение, указывающее, равен ли этот экземпляр заданному значению типа HeaderConsistency</summary>
        /// <param name="other"> Значение типа HeaderConsistency для сравнения с данным экземпляром</param>
        /// <returns>true, если значение параметра other совпадает со значением данного экземпляра;в противном случае — false</returns>
        public bool Equals(RecipeMixerSet other)
        {
            return IsUsed.Equals(other.IsUsed)
                && Name.Equals(other.Name)
                && TimeDischarge.Equals(other.TimeDischarge)
                && TimeExtraUnload.Equals(other.TimeExtraUnload)
                && UploadMode.Equals(other.UploadMode)
                && ImpulseCount.Equals(other.ImpulseCount)
                && ImpulseTime.Equals(other.ImpulseTime)
                && TimeBetweenImpulse.Equals(other.TimeBetweenImpulse)
                && NotClose.Equals(other.NotClose)
                && Delay50.Equals(other.Delay50)
                && Delay75.Equals(other.Delay75);
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

            return Equals((RecipeMixerSet)other);
        }

        /// <summary>
        /// хэш-код класса
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return (IsUsed, Name, TimeDischarge, TimeExtraUnload, UploadMode, ImpulseCount, ImpulseTime, TimeBetweenImpulse, NotClose, Delay50, Delay75).GetHashCode();
        }

        /// <summary>
        /// Сравнение двух наборов настроек смесителя
        /// </summary>
        /// <param name="item1">Настройки смесителя</param>
        /// <param name="item2">Настройки смесителя</param>
        /// <returns></returns>
        public static bool operator ==(RecipeMixerSet item1, RecipeMixerSet item2)
        {
            if (((object)item1) == null || ((object)item2) == null)
                return Object.Equals(item1, item2);

            return item1.Equals(item2);
        }

        /// <summary>
        /// Неравенство двух наборов настроек смесителя
        /// </summary>
        /// <param name="item1">Набор смесителя 1</param>
        /// <param name="item2">Набор смесителя 2</param>
        /// <returns></returns>
        public static bool operator !=(RecipeMixerSet item1, RecipeMixerSet item2)
        {
            if (((object)item1) == null || ((object)item2) == null)
                return !Object.Equals(item1, item2);

            return !(item1.Equals(item2));
        }

        #endregion IEquatable

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Набор настроек смесителя {Name} [ID = {Id}]"; // todo  как объект в журнале событий
        }

        /// <summary>
        /// Неявное преобразование типа <see cref="CreateMixerSetDto"/> к целевому типу <see cref="RecipeMixerSet"/>.
        /// </summary>
        /// <param name="model">Источник.</param>
        //public static explicit operator RecipeMixerSet(CreateMixerSetDto model)
        //{
        //    if (model == null) return null;

        //    return new RecipeMixerSet()
        //    {
        //        Guid = Guid.TryParse(model.Guid ?? string.Empty, out Guid guid) ? guid : Guid.Empty,

        //        TimeDischarge = model.TimeDischarge,
        //        UploadMode = (MixerUploadType)model.UploadMode,
        //        ImpulseCount = model.ImpulseCount,
        //        ImpulseTime = model.ImpulseTime,
        //        TimeBetweenImpulse = model.TimeBetweenImpulse,
        //        NotClose = model.NotClose,
        //        TimeExtraUnload = model.TimeExtraUnload,
        //        Delay50 = model.Delay50,
        //        Delay75 = model.Delay75,
        //    };
        //}

        /// <summary>
        /// Явное преобразование типа <see cref="UpdateMixerSetByIdDto"/> к типу <see cref="RecipeMixerSet"/>
        /// </summary>
        /// <param name="model">Источник</param>
        //public static explicit operator RecipeMixerSet(UpdateMixerSetByIdDto model)
        //{
        //    if (model == null) return null;

        //    return new RecipeMixerSet()
        //    {
        //        Id = model.Id,
        //        Name = model.Name,

        //        TimeDischarge = model.TimeDischarge,
        //        UploadMode = (MixerUploadType)model.UploadMode,
        //        ImpulseCount = model.ImpulseCount,
        //        ImpulseTime = model.ImpulseTime,
        //        TimeBetweenImpulse = model.TimeBetweenImpulse,
        //        NotClose = model.NotClose,
        //        TimeExtraUnload = model.TimeExtraUnload,
        //        Delay50 = model.Delay50,
        //        Delay75 = model.Delay75
        //    };
        //}

        /// <summary>
        /// Явное преобразование типа <see cref="UpdateMixerSetByGuidDto"/> к типу <see cref="RecipeMixerSet"/>.
        /// </summary>
        /// <param name="model">Источник.</param>
        //public static explicit operator RecipeMixerSet(UpdateMixerSetByGuidDto model)
        //{
        //    if (model == null) return null;

        //    return new RecipeMixerSet()
        //    {
        //        Guid = Guid.TryParse(model.Guid ?? string.Empty, out Guid guid) ? guid : Guid.Empty,
        //        Name = model.Name,

        //        TimeDischarge = model.TimeDischarge,
        //        UploadMode = (MixerUploadType)model.UploadMode,
        //        ImpulseCount = model.ImpulseCount,
        //        ImpulseTime = model.ImpulseTime,
        //        TimeBetweenImpulse = model.TimeBetweenImpulse,
        //        NotClose = model.NotClose,
        //        TimeExtraUnload = model.TimeExtraUnload,
        //        Delay50 = model.Delay50,
        //        Delay75 = model.Delay75
        //    };
        //}
    }
}
