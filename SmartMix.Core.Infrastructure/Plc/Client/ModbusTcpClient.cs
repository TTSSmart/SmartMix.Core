using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SmartMix.Core.Infrastructure.Plc.Client
{
    /// <summary>
    /// Драйвер контроллера PLC.
    /// </summary>
    public class ModbusTcpClient : IModbusTcpClient
    {
        private const byte fctReadCoil = 1;
        private const byte fctReadDiscreteInputs = 2;
        private const byte fctReadHoldingRegister = 3;
        private const byte fctReadInputRegister = 4;
        private const byte fctWriteSingleCoil = 5;
        private const byte fctWriteSingleRegister = 6;
        private const byte fctWriteMultipleCoils = 15;
        private const byte fctWriteMultipleRegister = 16;
        private const byte fctReadWriteMultipleRegister = 23;

        /// <summary>Количество попыток чтения/записи</summary>
        private const int MAX_COUNT = 3;

        /// <summary>Constant for exception illegal function.</summary>
        public const byte excIllegalFunction = 1;

        /// <summary>Constant for exception illegal data address.</summary>
        public const byte excIllegalDataAdr = 2;

        /// <summary>Constant for exception illegal data value.</summary>
        public const byte excIllegalDataVal = 3;

        /// <summary>Constant for exception slave device failure.</summary>
        public const byte excSlaveDeviceFailure = 4;

        /// <summary>Constant for exception acknowledge.</summary>
        public const byte excAck = 5;

        /// <summary>Constant for exception slave is busy/booting up.</summary>
        public const byte excSlaveIsBusy = 6;

        /// <summary>Constant for exception gate path unavailable.</summary>
        public const byte excGatePathUnavailable = 10;

        /// <summary>Constant for exception not connected.</summary>
        public const byte excExceptionNotConnected = 253;

        /// <summary>Constant for exception connection lost.</summary>
        public const byte excExceptionConnectionLost = 254;

        /// <summary>Constant for exception response timeout.</summary>
        public const byte excExceptionTimeout = 255;

        /// <summary>Constant for exception wrong offset.</summary>
        private const byte excExceptionOffset = 128;

        /// <summary>Constant for exception send failt.</summary>
        private const byte excSendFailt = 100;

        // ------------------------------------------------------------------------
        private readonly byte[] tcpSynClBuffer = new byte[2048];

        /// <summary>
        /// Представляет IP-адрес контроллера ПЛК.
        /// </summary>
        /// <value>Значение по умолчанию: 192.168.1.3</value>
        private readonly string _host;

        /// <summary>
        /// Представляет порт подключения к контроллеру ПЛК.
        /// </summary>
        /// <value>Значение по умолчанию: 502</value>
        private readonly int _port;

        /// <summary>
        /// Представляет TCP-клиент контроллера ПЛК.
        /// </summary>
        private TcpClient _tcpClient;

        /// <summary>
        /// Представляет поток обмена данными с контроллером ПЛК.
        /// </summary>
        private NetworkStream _networkStream;

        /// <summary>
        /// Инициализирует новый экземпляр класса по указанным параметрам.
        /// </summary>
        /// <param name="host">IP-адрес подключения.</param>
        /// <param name="port">Порт подключения.</param>
        /// <exception cref="ArgumentNullException">Исключение, которое генерируется, если не указаны параметры подключения.</exception>
        public ModbusTcpClient(string host = "192.168.1.3", int port = 502)
        {
            if (string.IsNullOrWhiteSpace(host))
                throw new ArgumentNullException(nameof(host));

            if (port < 1 && port > ushort.MaxValue)
                throw new ArgumentNullException(nameof(port));

            _host = host;
            _port = port;
        }

        /// <summary>Shows if a connection is active.</summary>
        /// <value>Значение<see langword="true"/>, если подключение с контроллером установлено, иначе - значение<see langword="false"/>.</value>
        public bool Connected => _tcpClient?.Client != null && _tcpClient.Client.Connected;

        /// <summary>Response data event. This event is called when new data arrives</summary>
        public event EventHandler<ModbusClientResponse> OnResponse;

        /// <summary>Exception data event. This event is called when the data is incorrect</summary>
        public event EventHandler<ModbusClientException> OnException;

        /// <summary>
        /// Выполняет попытку подключения к TCP-клиенту. Возвращает результат выполнения операции.
        /// </summary>
        /// <returns>Значение <see langword="true"/>, если связь с контроллером ПЛК успешно установлена, иначе -значение  <see langword="false"/>.</returns>
        public bool TryConnect()
        {
            try
            {
                Disconnect();

                _tcpClient = new TcpClient();
                _tcpClient.ConnectAsync(_host, _port).Wait(TimeSpan.FromSeconds(3));

                if (!_tcpClient.Connected)
                    throw new InvalidOperationException($"Не удалось установить соединение с контроллером {_host}:{_port}");

                _networkStream = _tcpClient.GetStream();
                _networkStream.ReadTimeout = 3000;
                _networkStream.WriteTimeout = 3000;

                return true;
            }
            catch (Exception ex)
            {
                _tcpClient?.Dispose();
                OnException?.Invoke(this, new ModbusClientException(ex));

                return false;
            }
        }

        /// <summary>Stop connection to slave.</summary>
        public void Disconnect()
        {
            try
            {
                if (_tcpClient != null)
                {
                    _tcpClient.GetStream()?.Close();
                    _tcpClient.Dispose();
                    //if (_tcpClient.Client != null && _tcpClient.Client.Connected)
                    //    _tcpClient.Close();
                }

                if (_networkStream != null)
                    _networkStream.Dispose();
            }
            catch (Exception)
            {
                // ignore?
            }
            finally
            {
                _tcpClient = null;
            }
        }

        /// <summary>Read holding registers from slave synchronous.</summary>
        /// <param name="id">Unique id that marks the transaction. In asynchronous mode this id is given to the callback function.</param>
        /// <param name="unit">
        ///     Unit identifier (previously slave address). In asynchronous mode this unit is given to the callback
        ///     function.
        /// </param>
        /// <param name="startAddress">Address from where the data read begins.</param>
        /// <param name="numInputs">Length of data.</param>
        /// <param name="response">Contains the result of function.</param>
        public void ReadHoldingRegister(ushort id, byte unit, ushort startAddress, ushort numInputs, ref byte[] response)
        {
            response = WriteSyncData(CreateReadHeader(id, unit, startAddress, numInputs, fctReadHoldingRegister), id);
        }

        /// <summary>Read input registers from slave synchronous.</summary>
        /// <param name="id">Unique id that marks the transaction. In asynchronous mode this id is given to the callback function.</param>
        /// <param name="unit">
        ///     Unit identifier (previously slave address). In asynchronous mode this unit is given to the callback
        ///     function.
        /// </param>
        /// <param name="startAddress">Address from where the data read begins.</param>
        /// <param name="numInputs">Length of data.</param>
        /// <param name="values">Contains the result of function.</param>
        public void ReadInputRegister(ushort id, byte unit, ushort startAddress, ushort numInputs, ref byte[] values)
        {
            values = WriteSyncData(CreateReadHeader(id, unit, startAddress, numInputs, fctReadInputRegister), id);
        }

        /// <summary>Write single coil in slave synchronous.</summary>
        /// <param name="id">Unique id that marks the transaction. In asynchronous mode this id is given to the callback function.</param>
        /// <param name="unit">
        ///     Unit identifier (previously slave address). In asynchronous mode this unit is given to the callback
        ///     function.
        /// </param>
        /// <param name="startAddress">Address from where the data read begins.</param>
        /// <param name="OnOff">Specifies if the coil should be switched on or off.</param>
        /// <param name="result">Contains the result of the synchronous write.</param>
        public void WriteSingleCoils(ushort id, byte unit, ushort startAddress, bool OnOff, ref byte[] result)
        {
            byte[] data;
            data = CreateWriteHeader(id, unit, startAddress, 1, 1, fctWriteSingleCoil);
            if (OnOff) data[10] = 255;
            else data[10] = 0;
            result = WriteSyncData(data, id);
        }

        /// <summary>Write multiple coils in slave synchronous.</summary>
        /// <param name="id">Unique id that marks the transaction. In asynchronous mode this id is given to the callback function.</param>
        /// <param name="unit">
        ///     Unit identifier (previously slave address). In asynchronous mode this unit is given to the callback
        ///     function.
        /// </param>
        /// <param name="startAddress">Address from where the data read begins.</param>
        /// <param name="numBits">Specifies number of bits.</param>
        /// <param name="values">Contains the bit information in byte format.</param>
        /// <param name="result">Contains the result of the synchronous write.</param>
        public void WriteMultipleCoils(ushort id, byte unit, ushort startAddress, ushort numBits, byte[] values,
            ref byte[] result)
        {
            byte numBytes = Convert.ToByte(values.Length);
            byte[] data;
            data = CreateWriteHeader(id, unit, startAddress, numBits, (byte)(numBytes + 2), fctWriteMultipleCoils);
            Array.Copy(values, 0, data, 13, numBytes);
            result = WriteSyncData(data, id);
        }

        /// <summary>Write single register in slave synchronous.</summary>
        /// <param name="id">Unique id that marks the transaction. In asynchronous mode this id is given to the callback function.</param>
        /// <param name="unit">
        ///     Unit identifier (previously slave address). In asynchronous mode this unit is given to the callback
        ///     function.
        /// </param>
        /// <param name="startAddress">Address to where the data is written.</param>
        /// <param name="values">Contains the register information.</param>
        /// <param name="result">Contains the result of the synchronous write.</param>
        public void WriteSingleRegister(ushort id, byte unit, ushort startAddress, byte[] values, ref byte[] result)
        {
            byte[] data;
            data = CreateWriteHeader(id, unit, startAddress, 1, 1, fctWriteSingleRegister);
            data[10] = values[0];
            data[11] = values[1];
            result = WriteSyncData(data, id);
        }

        /// <summary>Write multiple registers in slave synchronous.</summary>
        /// <param name="id">Unique id that marks the transaction. In asynchronous mode this id is given to the callback function.</param>
        /// <param name="unit">
        ///     Unit identifier (previously slave address). In asynchronous mode this unit is given to the callback
        ///     function.
        /// </param>
        /// <param name="startAddress">Address to where the data is written.</param>
        /// <param name="values">Contains the register information.</param>
        /// <param name="result">Contains the result of the synchronous write.</param>
        public void WriteMultipleRegister(ushort id, byte unit, ushort startAddress, byte[] values, ref byte[] result)
        {
            ushort numBytes = Convert.ToUInt16(values.Length);
            if (numBytes % 2 > 0) numBytes++;
            byte[] data;

            data = CreateWriteHeader(
                id,
                unit,
                startAddress,
                Convert.ToUInt16(numBytes / 2
                ),
                Convert.ToUInt16(numBytes + 2),
                fctWriteMultipleRegister);
            Array.Copy(values, 0, data, 13, values.Length);
            result = WriteSyncData(data, id);
        }

        /// <summary>Read/Write multiple registers in slave synchronous. The result is given in the response function.</summary>
        /// <param name="id">Unique id that marks the transaction. In asynchronous mode this id is given to the callback function.</param>
        /// <param name="unit">
        ///     Unit identifier (previously slave address). In asynchronous mode this unit is given to the callback
        ///     function.
        /// </param>
        /// <param name="startReadAddress">Address from where the data read begins.</param>
        /// <param name="numInputs">Length of data.</param>
        /// <param name="startWriteAddress">Address to where the data is written.</param>
        /// <param name="values">Contains the register information.</param>
        /// <param name="result">Contains the result of the synchronous command.</param>
        public void ReadWriteMultipleRegister(ushort id, byte unit, ushort startReadAddress, ushort numInputs,
            ushort startWriteAddress, byte[] values, ref byte[] result)
        {
            ushort numBytes = Convert.ToUInt16(values.Length);
            if (numBytes % 2 > 0) numBytes++;
            byte[] data;

            data = CreateReadWriteHeader(id, unit, startReadAddress, numInputs, startWriteAddress,
                Convert.ToUInt16(numBytes / 2));
            Array.Copy(values, 0, data, 17, values.Length);
            result = WriteSyncData(data, id);
        }

        // Create modbus header for read action
        private byte[] CreateReadHeader(ushort id, byte unit, ushort startAddress, ushort length, byte function)
        {
            var data = new byte[12];

            byte[] _id = BitConverter.GetBytes((short)id);
            data[0] = _id[1]; // Slave id high byte
            data[1] = _id[0]; // Slave id low byte
            data[5] = 6; // Message size
            data[6] = unit; // Slave address
            data[7] = function; // Function code
            byte[] _adr = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)startAddress));
            data[8] = _adr[0]; // Start address
            data[9] = _adr[1]; // Start address
            byte[] _length = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)length));
            data[10] = _length[0]; // Number of data to read
            data[11] = _length[1]; // Number of data to read
            return data;
        }

        // Create modbus header for write action
        private byte[] CreateWriteHeader(ushort id, byte unit, ushort startAddress, ushort numData, ushort numBytes,
            byte function)
        {
            var data = new byte[numBytes + 11];

            byte[] _id = BitConverter.GetBytes((short)id);
            data[0] = _id[1]; // Slave id high byte
            data[1] = _id[0]; // Slave id low byte
            byte[] _size = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)(5 + numBytes)));
            data[4] = _size[0]; // Complete message size in bytes
            data[5] = _size[1]; // Complete message size in bytes
            data[6] = unit; // Slave address
            data[7] = function; // Function code
            byte[] _adr = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)startAddress));
            data[8] = _adr[0]; // Start address
            data[9] = _adr[1]; // Start address
            if (function >= fctWriteMultipleCoils)
            {
                byte[] _cnt = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)numData));
                data[10] = _cnt[0]; // Number of bytes
                data[11] = _cnt[1]; // Number of bytes
                data[12] = (byte)(numBytes - 2);
            }
            return data;
        }

        // Create modbus header for read/write action
        private byte[] CreateReadWriteHeader(ushort id, byte unit, ushort startReadAddress, ushort numRead,
            ushort startWriteAddress, ushort numWrite)
        {
            var data = new byte[numWrite * 2 + 17];

            byte[] _id = BitConverter.GetBytes((short)id);
            data[0] = _id[1]; // Slave id high byte
            data[1] = _id[0]; // Slave id low byte
            byte[] _size = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)(11 + numWrite * 2)));
            data[4] = _size[0]; // Complete message size in bytes
            data[5] = _size[1]; // Complete message size in bytes
            data[6] = unit; // Slave address
            data[7] = fctReadWriteMultipleRegister; // Function code
            byte[] _adr_read = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)startReadAddress));
            data[8] = _adr_read[0]; // Start read address
            data[9] = _adr_read[1]; // Start read address
            byte[] _cnt_read = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)numRead));
            data[10] = _cnt_read[0]; // Number of bytes to read
            data[11] = _cnt_read[1]; // Number of bytes to read
            byte[] _adr_write = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)startWriteAddress));
            data[12] = _adr_write[0]; // Start write address
            data[13] = _adr_write[1]; // Start write address
            byte[] _cnt_write = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)numWrite));
            data[14] = _cnt_write[0]; // Number of bytes to write
            data[15] = _cnt_write[1]; // Number of bytes to write
            data[16] = (byte)(numWrite * 2);

            return data;
        }

        // Write data and and wait for response
        private byte[] WriteSyncData(byte[] write_data, ushort id)
        {
            return TryWriteSyncData(write_data, id);
        }

        private byte[] TryWriteSyncData(byte[] write_data, ushort id)
        {
            for (int i = 0; i < MAX_COUNT; i++)
            {
                try
                {
                    _networkStream.Write(write_data, 0, write_data.Length);
                    int result = _networkStream.Read(tcpSynClBuffer, 0, tcpSynClBuffer.Length);

                    byte unit = tcpSynClBuffer[6];
                    byte function = tcpSynClBuffer[7];
                    byte[] data;

                    if (result == 0)
                        throw new IOException("Соединение потеряно");

                    // Response data is slave exception
                    if (function > excExceptionOffset)
                    {
                        function -= excExceptionOffset;
                        OnException?.Invoke(this, new ModbusClientException(id, unit, function, tcpSynClBuffer[8]));
                        throw new IOException($"Соединение потеряно. f=[{function}]");
                    }

                    // Write response data
                    if ((function >= fctWriteSingleCoil) && (function != fctReadWriteMultipleRegister))
                    {
                        data = new byte[2];
                        Array.Copy(tcpSynClBuffer, 10, data, 0, 2);
                    }

                    // Read response data
                    else
                    {
                        data = new byte[tcpSynClBuffer[8]];
                        Array.Copy(tcpSynClBuffer, 9, data, 0, tcpSynClBuffer[8]);
                    }
                    OnResponse?.Invoke(this, new ModbusClientResponse(id, unit, function, data));
                    return data;
                }
                catch (IOException ex)
                {
                    OnException?.Invoke(this, new ModbusClientException(ex));
                    TryConnect();
                }
            }
            throw new IOException("Соединение потеряно");
        }

        #region IDisposable Members

        private bool _disposed;

        /// <summary>
        /// Разрывает соединение с контроллером ПЛК.
        /// </summary>
        /// <param name="disposing">Значение <see langword="true"/>, если метод вызывается из Dispose(), значение <see langword="false"/>, если метод вызывается из метода завершения.</param>
        private void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                // Dispose managed resources
                try
                {
                    Disconnect();
                }
                catch (Exception)
                {
                    // ignore
                }
            }

            // Free native resources
            _disposed = true;
        }

        /// <summary>Destroy master instance</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Members

        ~ModbusTcpClient()
        {
            Dispose(false);
        }
    }

    /// <summary>
    /// Аргументы события <see cref="IModbusTcpClient.OnException"/>
    /// </summary>
    public class ModbusClientException : EventArgs
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса с ошибкой <paramref name="error"/>.
        /// </summary>
        /// <param name="error">Текст ошибки.</param>
        public ModbusClientException(string error)
        {
            Message = error;
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса для указанного исключения.
        /// </summary>
        /// <param name="ex">Исключение.</param>
        public ModbusClientException(Exception ex)
        {
            Message = ex.GetBaseException().Message;
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса по указанным параметрам.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="unit"></param>
        /// <param name="function"></param>
        /// <param name="exception"></param>
        public ModbusClientException(ushort id, byte unit, byte function, byte exception)
        {
            Id = id;
            Unit = unit;
            Function = function;
            Exception = exception;
        }

        public ushort Id { get; }
        public byte Unit { get; }
        public byte Function { get; }
        public byte Exception { get; }

        /// <summary>
        /// Возвращает или задаёт текст ошибки.
        /// </summary>
        public string Message { get; private set; }
    }

    /// <summary>
    /// Аргументы события <see cref="IModbusTcpClient.OnResponse"/>
    /// </summary>
    public class ModbusClientResponse : EventArgs
    {
        public ModbusClientResponse(ushort id, byte unit, byte function, byte[] data)
        {
            Id = id;
            Unit = unit;
            Function = function;
            Data = data;
        }

        public ushort Id { get; }
        public byte Unit { get; }
        public byte Function { get; }
        public byte[] Data { get; }
    }
}
