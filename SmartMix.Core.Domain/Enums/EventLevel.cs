using System.ComponentModel;
using System.Runtime.Serialization;

namespace SmartMix.Core.Domain.Enums
{
    /// <summary>
    /// Уровень события
    /// </summary>
    /// <remarks>Фиксируется в event_types</remarks>
    [DataContract]
    public enum EventLevel
    {
        /// <summary>
        /// Авария
        /// </summary>
        [Description("Авария")]
        [EnumMember]
        Accident = 10,

        /// <summary>
        /// Системная ошибка
        /// </summary>
        [Description("Ошибка")]
        [EnumMember]
        SystemError = 20,

        /// <summary>
        /// Служебная информация
        /// </summary>
        [Description("Информация")]
        [EnumMember]
        Information = 30,

        /// <summary>
        /// Настройки и параметры, команды
        /// </summary>
        [Description("Настройки и команды")]
        [EnumMember]
        Settings = 40
    }
}
