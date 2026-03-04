namespace SmartMix.Core.Infrastructure.Plc.Client
{
    public class ModbusTcpClientMock : IModbusTcpClient
    {
        public ModbusTcpClientMock(string host, int port)
        {
            Console.BackgroundColor = ConsoleColor.Red;
            Console.WriteLine($"[{nameof(ModbusTcpClientMock)}] ЗАПУСК ЗАГЛУШКИ ПЛК {host}:{port}");
            Console.BackgroundColor = ConsoleColor.Black;

        }
        public bool Connected => true;

        public event EventHandler<ModbusClientResponse> OnResponse;
        public event EventHandler<ModbusClientException> OnException;

        public bool TryConnect()
        {
            return true;
        }

        /// <summary>Stop connection to slave.</summary>
        public void Disconnect()
        {
        }

        /// <summary>Destroy master instance</summary>
        public void Dispose()
        {
            Disconnect();
        }

        public void ReadHoldingRegister(ushort id, byte unit, ushort startAddress, ushort numInputs, ref byte[] response)
        {
            response = new byte[byte.MaxValue + 1];
        }

        public void ReadInputRegister(ushort id, byte unit, ushort startAddress, ushort numInputs, ref byte[] values)
        {
            values = new byte[byte.MaxValue];
        }

        public void WriteSingleCoils(ushort id, byte unit, ushort startAddress, bool OnOff, ref byte[] result)
        {
            result = new byte[byte.MaxValue];
        }

        public void WriteMultipleCoils(ushort id, byte unit, ushort startAddress, ushort numBits, byte[] values,
            ref byte[] result)
        {
            result = new byte[byte.MaxValue];
        }

        public void WriteSingleRegister(ushort id, byte unit, ushort startAddress, byte[] values, ref byte[] result)
        {
            result = new byte[byte.MaxValue];
        }
        public void WriteMultipleRegister(ushort id, byte unit, ushort startAddress, byte[] values, ref byte[] result)
        {
            result = new byte[byte.MaxValue];
        }

        public void ReadWriteMultipleRegister(ushort id, byte unit, ushort startReadAddress, ushort numInputs,
            ushort startWriteAddress, byte[] values, ref byte[] result)
        {
            result = new byte[byte.MaxValue];
        }
    }
}
