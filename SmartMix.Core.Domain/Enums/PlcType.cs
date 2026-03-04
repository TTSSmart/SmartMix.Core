using System.Runtime.Serialization;

namespace SmartMix.Core.Domain.Enums
{
    /// <summary>
    /// Тип переменных, используемых в PLC
    /// </summary>
    [DataContract]
    public enum PlcType
    {
        /// <summary>
        /// Массив
        /// </summary>
        [EnumMember]
        Array,

        /// <summary>
        /// Булево значение
        /// </summary>
        [EnumMember]
        Bool,

        /// <summary>
        /// Указатель Word
        /// </summary>
        [EnumMember]
        Word
    }
}
