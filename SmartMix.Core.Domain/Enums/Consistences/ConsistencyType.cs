using System.ComponentModel;
using System.Runtime.Serialization;

namespace SmartMix.Core.Domain.Enums.Consistences
{
    /// <summary>Указывает на марку подвижности, или показатель подвижности</summary>
    [DataContract]
    public enum ConsistencyType
    {
        /// <summary>П1</summary>
        /// <value>Осадка конуса 1-4см</value>
        [Description("П1"), EnumMember]
        P1 = 0,
        /// <summary>П2</summary>
        /// <value>Осадка конуса 5-9см</value>
        [Description("П2"), EnumMember]
        P2 = 1,
        /// <summary>П3</summary>
        /// <value>Осадка конуса 10-15см</value>
        [Description("П3"), EnumMember]
        P3 = 2,
        /// <summary>П4</summary>
        /// <value>Осадка конуса 16-20см</value>
        [Description("П4"), EnumMember]
        P4 = 3,
        /// <summary>П5</summary>
        /// <value>Осадка конуса больше 20 см</value>
        [Description("П5"), EnumMember]
        P5 = 4,
        /// <summary>П6</summary>
        [Description("П6"), EnumMember]
        P6 = 5,
        /// <summary>SVB</summary>
        [Description("SVB"), EnumMember]
        SVB = 6,
        /// <summary>Не задана</summary>
        [Description("None"), EnumMember]
        None = 7
    }
}
