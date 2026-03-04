using System.ComponentModel;
using System.Runtime.Serialization;

namespace SmartMix.Core.Domain.Enums
{
    /// <summary>
    /// Указывает на уровень влажности
    /// </summary>
    [DataContract]
    public enum LevelHumidity
    {
        /// <summary>
        /// Минимальный
        /// </summary>
        [Description("Минимальный")]
        [EnumMember]
        Min = 0,

        /// <summary>
        /// Средний
        /// </summary>
        [Description("Средний")]
        [EnumMember]
        Middle = 1,

        /// <summary>
        /// Максимальный
        /// </summary>
        [Description("Максимальный")]
        [EnumMember]
        Max = 2
    }
}
