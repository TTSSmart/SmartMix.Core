using SmartMix.Core.Domain.Entities.Base;
using SmartMix.Core.Domain.Entities.Base.Shared;
using SmartMix.Core.Domain.Enums;
using System.Runtime.Serialization;

namespace SmartMix.Core.Domain.Entities.Errors
{
    /// <summary>
    /// Ошибка объекта
    /// </summary>
    [DataContract]
    public class Error : BaseInfo, ICloneable<Error>
    {
        /// <summary>Номер бита</summary>
        [DataMember(IsRequired = true)]
        public int Bits { get; set; }

        /// <summary>Название сетевой</summary>
        [DataMember(IsRequired = true)]
        public string PlcName { get; set; }

        /// <summary>Используемый тип сетевой</summary>
        [DataMember]
        public PlcType PlcType { get; set; }

        /// <summary>
        /// Клонирование
        /// </summary>
        /// <returns></returns>
        public new Error Clone()
        {
            return (Error)MemberwiseClone();
        }
    }
}
