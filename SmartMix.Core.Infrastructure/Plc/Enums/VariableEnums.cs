using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartMix.Core.Infrastructure.Plc.Enums
{
    /// <summary> Типы регистров на PLC </summary>
    public enum VariableType
    {
        /// <summary>
        /// Word, Int, UInt : 16 бит
        /// </summary>
        [Description("INT|UINT|WORD")]
        Int,

        /// <summary>
        /// DWord, DInt, UDInt : 32 бита
        /// </summary>
        [Description("DINT|UDINT|DWORD")]
        Uint,

        /// <summary>
        /// Real, 32 бита
        /// </summary>
        [Description("REAL")]
        Float,

        /// <summary>
        /// Bool, 1 бит
        /// </summary>
        [Description("BOOL")]
        Bool,

        /// <summary>
        /// Array, n * 16 бит
        /// </summary>
        [Description("ARRAY")]
        Array
    }
}
