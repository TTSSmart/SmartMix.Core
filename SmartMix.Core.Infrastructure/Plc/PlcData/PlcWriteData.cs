namespace SmartMix.Core.Infrastructure.Plc.PlcData
{
    /// <summary>
    /// Данные для записи в PLC
    /// </summary>
    internal struct PlcWriteData
    {
        public PlcWriteData(ushort address, ushort[] value)
        {
            Address = address;
            Value = value;
        }

        /// <summary>
        /// Начальный адрес для записи
        /// </summary>
        public ushort Address { get; }

        /// <summary>
        /// Данные
        /// </summary>
        public ushort[] Value { get; }
    }
}
