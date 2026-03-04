using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SmartMix.Core.Domain.Enums.Consistences
{
    /// <summary>Указывает на вид отображения консистенции.</summary>
    [DataContract]
    public enum ConsistencyDisplay
    {
        /// <summary>Отображать осадку конуса, в сантиметрах</summary>
        [Description("ОК"), EnumMember]
        Conus,
        /// <summary>Отображать марку подвижности</summary>
        [Description("Марка"), EnumMember]
        Brand
    }
}
