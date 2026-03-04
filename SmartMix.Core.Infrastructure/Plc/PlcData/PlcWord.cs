namespace SmartMix.Core.Infrastructure.Plc.PlcData
{
    /// <summary>
    /// Ячейка памяти на PLC
    /// </summary>
    internal class PlcWord
    {
        public PlcWord(ushort address)
        {
            Address = address;
        }
        /// <summary>
        /// Адрес
        /// </summary>
        public ushort Address { get; private set; }
        /// <summary>
        /// Значение
        /// </summary>
        public ushort Value { get; set; }
    }
}
