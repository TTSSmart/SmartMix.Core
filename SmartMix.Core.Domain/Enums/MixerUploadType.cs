using System.ComponentModel;
using System.Runtime.Serialization;

namespace SmartMix.Core.Domain.Enums
{
    /// <summary>
    /// Тип выгрузки из смесителя
    /// </summary>
    [DataContract]
    public enum MixerUploadType
    {
        /// <summary>Постоянный</summary>        
        [Description("Постоянный"), EnumMember]
        Constant = 1,

        /// <summary>импульсный</summary>        
        [Description("Импульсный"), EnumMember]
        Impulse = 2
    }
}
