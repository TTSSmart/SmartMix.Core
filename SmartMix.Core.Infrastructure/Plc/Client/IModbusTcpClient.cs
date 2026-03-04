namespace SmartMix.Core.Infrastructure.Plc.Client
{
    /// <summary>
    /// Интерфейс драйвера контроллера PLC.
    /// </summary>
    public interface IModbusTcpClient : IDisposable
    {
        /// <summary>
        /// Возвращает признак наличия подключения с контроллером ПЛК.
        /// </summary>
        bool Connected { get; }
        event EventHandler<ModbusClientResponse> OnResponse;
        event EventHandler<ModbusClientException> OnException;

        /// <summary>
        /// Выполняет попытку подключения к контроллеру ПЛК. Возвращает результат выполнения операции.
        /// </summary>
        /// <returns>Значение <see langword="true"/>, если связь с контроллером ПЛК успешно установлена, иначе -значение  <see langword="false"/>.</returns>
        bool TryConnect();

        /// <summary>
        /// Закрывает соединение с контроллером ПЛК.
        /// </summary>
        void Disconnect();
        void ReadHoldingRegister(ushort id, byte unit, ushort startAddress, ushort numInputs, ref byte[] response);
        void ReadInputRegister(ushort id, byte unit, ushort startAddress, ushort numInputs, ref byte[] values);
        void WriteSingleCoils(ushort id, byte unit, ushort startAddress, bool OnOff, ref byte[] result);
        void WriteMultipleCoils(ushort id, byte unit, ushort startAddress, ushort numBits, byte[] values, ref byte[] result);
        void WriteSingleRegister(ushort id, byte unit, ushort startAddress, byte[] values, ref byte[] result);
        void WriteMultipleRegister(ushort id, byte unit, ushort startAddress, byte[] values, ref byte[] result);
        void ReadWriteMultipleRegister(ushort id, byte unit, ushort startReadAddress, ushort numInputs, ushort startWriteAddress, byte[] values, ref byte[] result);
    }
}
