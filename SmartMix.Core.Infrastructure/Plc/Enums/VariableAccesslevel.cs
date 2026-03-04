using System.ComponentModel;

namespace SmartMix.Core.Infrastructure.Plc.Enums
{
    /// <summary>
    /// Указывает на доступ к переменной PLC.
    /// </summary>
    public enum VariableAccessLevel
    {
        /// <summary>
        /// Доступ на чтение.
        /// </summary>
        [Description("R")]
        Read,

        /// <summary>
        /// Доступ на чтение и запись.
        /// </summary>
        [Description("RW")]
        ReadWrite
    }
}
