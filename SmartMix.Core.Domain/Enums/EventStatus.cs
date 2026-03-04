using System.ComponentModel;
using System.Runtime.Serialization;

namespace SmartMix.Core.Domain.Enums
{
    /// <summary>
    /// Статус события
    /// </summary>
    [DataContract]
    public enum EventStatus
    {
        /// <summary>Не авария. Значение по умолчанию.</summary>
        [Description("")]
        [EnumMember]
        None,

        /// <summary>Авария подтверждена  (при просмотре в в журнале событий).</summary>
        [Description("Авария подтверждена")]
        [EnumMember]
        AccidentConfirmed,

        /// <summary>Авария не подтверждена  (при просмотре в в журнале событий).</summary>
        [Description("Авария не подтверждена")]
        [EnumMember]
        AccidentNotConfirmed
    }
}
