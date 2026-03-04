using System.Net;
using System.Net.Sockets;

namespace SmartMix.Core.Infrastructure.Plc.Client
{
    /// <summary>
    ///     Modbus TCP common driver class. This class implements a modbus TCP master driver.
    ///     It supports the following commands:
    ///     Read coils
    ///     Read discrete inputs
    ///     Write single coil
    ///     Write multiple cooils
    ///     Read holding register
    ///     Read input register
    ///     Write single register
    ///     Write multiple register
    ///     All commands can be sent in synchronous or asynchronous mode. If a value is accessed
    ///     in synchronous mode the program will stop and wait for slave to response. If the
    ///     slave didn't answer within a specified time a timeout exception is called.
    ///     The class uses multi threading for both synchronous and asynchronous access. For
    ///     the communication two lines are created. This is necessary because the synchronous
    ///     thread has to wait for a previous command to finish.
    /// </summary>
    public class Master
    {
        #region Events

        /// <summary>Response data event. This event is called when new data arrives</summary>
        public delegate void ResponseData(ushort id, byte unit, byte function, byte[] data);

        /// <summary>Response data event. This event is called when new data arrives</summary>
        public event ResponseData OnResponseData;

        /// <summary>Exception data event. This event is called when the data is incorrect</summary>
        public delegate void ExceptionData(ushort id, byte unit, byte function, byte exception);

        /// <summary>Exception data event. This event is called when the data is incorrect</summary>
        public event ExceptionData OnException;

        #endregion Events

        private const byte fctReadCoil = 1;
        private const byte fctReadDiscreteInputs = 2;
        private const byte fctReadHoldingRegister = 3;
        private const byte fctReadInputRegister = 4;
        private const byte fctWriteSingleCoil = 5;
        private const byte fctWriteSingleRegister = 6;
        private const byte fctWriteMultipleCoils = 15;
        private const byte fctWriteMultipleRegister = 16;
        private const byte fctReadWriteMultipleRegister = 23;

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

        private bool _connected;

        private readonly byte[] _tcpAsyClBuffer = new byte[2048];
        private readonly byte[] _tcpSynClBuffer = new byte[2048];

        private Socket _tcpAsyCl;
        private Socket _tcpSynCl;

        /// <summary>Create master instance without parameters.</summary>
        public Master()
        {
        }

        /// <summary>Create master instance with parameters.</summary>
        /// <param name="ip">IP address of modbus slave.</param>
        /// <param name="port">Port number of modbus slave. Usually port 502 is used.</param>
        public Master(string ip, ushort port)
        {
            Connect(ip, port);
        }

        #region Properties


        private ushort _timeout = 500;

        /// <summary>Response timeout. If the slave didn't answers within in this time an exception is called.</summary>
        /// <value>The default value is 500ms.</value>
        public ushort Timeout
        {
            get { return _timeout; }
            set { _timeout = value; }
        }

        private ushort _refresh = 10;

        /// <summary>Refresh timer for slave answer. The class is polling for answer every X ms.</summary>
        /// <value>The default value is 10ms.</value>
        public ushort Refresh
        {
            get { return _refresh; }
            set { _refresh = value; }
        }

        /// <summary>Shows if a connection is active.</summary>
        public bool Connected
        {
            get { return _connected; }
        }

        #endregion Properties

        /// <summary>Start connection to slave.</summary>
        /// <param name="ip">IP address of modbus slave.</param>
        /// <param name="port">Port number of modbus slave. Usually port 502 is used.</param>
        public void Connect(string ip, ushort port)
        {
            try
            {
                IPAddress _ip;
                if (IPAddress.TryParse(ip, out _ip) == false)
                {
                    IPHostEntry hst = Dns.GetHostEntry(ip);
                    ip = hst.AddressList[0].ToString();
                }
                // ----------------------------------------------------------------
                // Connect asynchronous client
                _tcpAsyCl = new Socket(IPAddress.Parse(ip).AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                IAsyncResult result = _tcpAsyCl.BeginConnect(_ip, port, null, null);

                bool success = result.AsyncWaitHandle.WaitOne(5000, true);
                if (!success)
                {
                    throw new IOException(); // Connection timed out.
                }

                _tcpAsyCl.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, _timeout);
                _tcpAsyCl.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, _timeout);
                _tcpAsyCl.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.NoDelay, 1);
                // ----------------------------------------------------------------
                // Connect synchronous client
                _tcpSynCl = new Socket(IPAddress.Parse(ip).AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                //tcpSynCl.Connect(new IPEndPoint(IPAddress.Parse(ip), port));


                IAsyncResult result2 = _tcpSynCl.BeginConnect(_ip, port, null, null);

                bool success2 = result2.AsyncWaitHandle.WaitOne(5000, true);
                if (!success2)
                {
                    throw new IOException(); // Connection timed out.
                }

                _tcpSynCl.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, _timeout);
                _tcpSynCl.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, _timeout);
                _tcpSynCl.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.NoDelay, 1);
                _connected = true;
            }
            catch (IOException error)
            {
                _connected = false;
                throw (error);
            }
        }

        /// <summary>Stop connection to slave.</summary>
        public void Disconnect()
        {
            Dispose(); // todo переделать на внутренний вызов
        }

        internal void CallException(ushort id, byte unit, byte function, byte exception)
        {
            if ((_tcpAsyCl == null) || (_tcpSynCl == null)) return;
            if (exception == excExceptionConnectionLost)
            {
                _tcpSynCl = null;
                _tcpAsyCl = null;
                _connected = false;
            }
            if (OnException != null) OnException(id, unit, function, exception);
        }

        private static UInt16 SwapUInt16(UInt16 inValue)
        {
            return (UInt16)(((inValue & 0xff00) >> 8) |
                             ((inValue & 0x00ff) << 8));
        }

        /// <summary>Read coils from slave asynchronous. The result is given in the response function.</summary>
        /// <param name="id">Unique id that marks the transaction. In asynchronous mode this id is given to the callback function.</param>
        /// <param name="unit">
        ///     Unit identifier (previously slave address). In asynchronous mode this unit is given to the callback
        ///     function.
        /// </param>
        /// <param name="startAddress">Address from where the data read begins.</param>
        /// <param name="numInputs">Length of data.</param>
        public void ReadCoils(ushort id, byte unit, ushort startAddress, ushort numInputs)
        {
            WriteAsyncData(CreateReadHeader(id, unit, startAddress, numInputs, fctReadCoil), id);
        }

        /// <summary>Read coils from slave synchronous.</summary>
        /// <param name="id">Unique id that marks the transaction. In asynchronous mode this id is given to the callback function.</param>
        /// <param name="unit">
        ///     Unit identifier (previously slave address). In asynchronous mode this unit is given to the callback
        ///     function.
        /// </param>
        /// <param name="startAddress">Address from where the data read begins.</param>
        /// <param name="numInputs">Length of data.</param>
        /// <param name="values">Contains the result of function.</param>
        public void ReadCoils(ushort id, byte unit, ushort startAddress, ushort numInputs, ref byte[] values)
        {
            values = WriteSyncData(CreateReadHeader(id, unit, startAddress, numInputs, fctReadCoil), id);
        }

        /// <summary>Read discrete inputs from slave asynchronous. The result is given in the response function.</summary>
        /// <param name="id">Unique id that marks the transaction. In asynchronous mode this id is given to the callback function.</param>
        /// <param name="unit">
        ///     Unit identifier (previously slave address). In asynchronous mode this unit is given to the callback
        ///     function.
        /// </param>
        /// <param name="startAddress">Address from where the data read begins.</param>
        /// <param name="numInputs">Length of data.</param>
        public void ReadDiscreteInputs(ushort id, byte unit, ushort startAddress, ushort numInputs)
        {
            WriteAsyncData(CreateReadHeader(id, unit, startAddress, numInputs, fctReadDiscreteInputs), id);
        }

        /// <summary>Read discrete inputs from slave synchronous.</summary>
        /// <param name="id">Unique id that marks the transaction. In asynchronous mode this id is given to the callback function.</param>
        /// <param name="unit">
        ///     Unit identifier (previously slave address). In asynchronous mode this unit is given to the callback
        ///     function.
        /// </param>
        /// <param name="startAddress">Address from where the data read begins.</param>
        /// <param name="numInputs">Length of data.</param>
        /// <param name="values">Contains the result of function.</param>
        public void ReadDiscreteInputs(ushort id, byte unit, ushort startAddress, ushort numInputs, ref byte[] values)
        {
            values = WriteSyncData(CreateReadHeader(id, unit, startAddress, numInputs, fctReadDiscreteInputs), id);
        }

        /// <summary>Read holding registers from slave asynchronous. The result is given in the response function.</summary>
        /// <param name="id">Unique id that marks the transaction. In asynchronous mode this id is given to the callback function.</param>
        /// <param name="unit">
        ///     Unit identifier (previously slave address). In asynchronous mode this unit is given to the callback
        ///     function.
        /// </param>
        /// <param name="startAddress">Address from where the data read begins.</param>
        /// <param name="numInputs">Length of data.</param>
        public void ReadHoldingRegister(ushort id, byte unit, ushort startAddress, ushort numInputs)
        {
            WriteAsyncData(CreateReadHeader(id, unit, startAddress, numInputs, fctReadHoldingRegister), id);
        }

        /// <summary>Read holding registers from slave synchronous.</summary>
        /// <param name="id">Unique id that marks the transaction. In asynchronous mode this id is given to the callback function.</param>
        /// <param name="unit">
        ///     Unit identifier (previously slave address). In asynchronous mode this unit is given to the callback
        ///     function.
        /// </param>
        /// <param name="startAddress">Address from where the data read begins.</param>
        /// <param name="numInputs">Length of data.</param>
        /// <param name="values">Contains the result of function.</param>
        public void ReadHoldingRegister(ushort id, byte unit, ushort startAddress, ushort numInputs, ref byte[] values)
        {
            values = WriteSyncData(CreateReadHeader(id, unit, startAddress, numInputs, fctReadHoldingRegister), id);
        }

        /// <summary>Read input registers from slave asynchronous. The result is given in the response function.</summary>
        /// <param name="id">Unique id that marks the transaction. In asynchronous mode this id is given to the callback function.</param>
        /// <param name="unit">
        ///     Unit identifier (previously slave address). In asynchronous mode this unit is given to the callback
        ///     function.
        /// </param>
        /// <param name="startAddress">Address from where the data read begins.</param>
        /// <param name="numInputs">Length of data.</param>
        public void ReadInputRegister(ushort id, byte unit, ushort startAddress, ushort numInputs)
        {
            WriteAsyncData(CreateReadHeader(id, unit, startAddress, numInputs, fctReadInputRegister), id);
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

        /// <summary>Write single coil in slave asynchronous. The result is given in the response function.</summary>
        /// <param name="id">Unique id that marks the transaction. In asynchronous mode this id is given to the callback function.</param>
        /// <param name="unit">
        ///     Unit identifier (previously slave address). In asynchronous mode this unit is given to the callback
        ///     function.
        /// </param>
        /// <param name="startAddress">Address from where the data read begins.</param>
        /// <param name="OnOff">Specifies if the coil should be switched on or off.</param>
        public void WriteSingleCoils(ushort id, byte unit, ushort startAddress, bool OnOff)
        {
            byte[] data;
            data = CreateWriteHeader(id, unit, startAddress, 1, 1, fctWriteSingleCoil);
            if (OnOff) data[10] = 255;
            else data[10] = 0;
            WriteAsyncData(data, id);
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

        /// <summary>Write multiple coils in slave asynchronous. The result is given in the response function.</summary>
        /// <param name="id">Unique id that marks the transaction. In asynchronous mode this id is given to the callback function.</param>
        /// <param name="unit">
        ///     Unit identifier (previously slave address). In asynchronous mode this unit is given to the callback
        ///     function.
        /// </param>
        /// <param name="startAddress">Address from where the data read begins.</param>
        /// <param name="numBits">Specifies number of bits.</param>
        /// <param name="values">Contains the bit information in byte format.</param>
        public void WriteMultipleCoils(ushort id, byte unit, ushort startAddress, ushort numBits, byte[] values)
        {
            byte numBytes = Convert.ToByte(values.Length);
            byte[] data;
            data = CreateWriteHeader(id, unit, startAddress, numBits, (byte)(numBytes + 2), fctWriteMultipleCoils);
            Array.Copy(values, 0, data, 13, numBytes);
            WriteAsyncData(data, id);
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

        /// <summary>Write single register in slave asynchronous. The result is given in the response function.</summary>
        /// <param name="id">Unique id that marks the transaction. In asynchronous mode this id is given to the callback function.</param>
        /// <param name="unit">
        ///     Unit identifier (previously slave address). In asynchronous mode this unit is given to the callback
        ///     function.
        /// </param>
        /// <param name="startAddress">Address to where the data is written.</param>
        /// <param name="values">Contains the register information.</param>
        public void WriteSingleRegister(ushort id, byte unit, ushort startAddress, byte[] values)
        {
            byte[] data;
            data = CreateWriteHeader(id, unit, startAddress, 1, 1, fctWriteSingleRegister);
            data[10] = values[0];
            data[11] = values[1];
            WriteAsyncData(data, id);
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

        /// <summary>Write multiple registers in slave asynchronous. The result is given in the response function.</summary>
        /// <param name="id">Unique id that marks the transaction. In asynchronous mode this id is given to the callback function.</param>
        /// <param name="unit">
        ///     Unit identifier (previously slave address). In asynchronous mode this unit is given to the callback
        ///     function.
        /// </param>
        /// <param name="startAddress">Address to where the data is written.</param>
        /// <param name="values">Contains the register information.</param>
        public void WriteMultipleRegister(ushort id, byte unit, ushort startAddress, byte[] values)
        {
            ushort numBytes = Convert.ToUInt16(values.Length);
            if (numBytes % 2 > 0) numBytes++;
            byte[] data;

            data = CreateWriteHeader(id, unit, startAddress, Convert.ToUInt16(numBytes / 2),
                Convert.ToUInt16(numBytes + 2), fctWriteMultipleRegister);
            Array.Copy(values, 0, data, 13, values.Length);
            WriteAsyncData(data, id);
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

            data = CreateWriteHeader(id, unit, startAddress, Convert.ToUInt16(numBytes / 2),
                Convert.ToUInt16(numBytes + 2), fctWriteMultipleRegister);
            Array.Copy(values, 0, data, 13, values.Length);
            result = WriteSyncData(data, id);
        }

        /// <summary>Read/Write multiple registers in slave asynchronous. The result is given in the response function.</summary>
        /// <param name="id">Unique id that marks the transaction. In asynchronous mode this id is given to the callback function.</param>
        /// <param name="unit">
        ///     Unit identifier (previously slave address). In asynchronous mode this unit is given to the callback
        ///     function.
        /// </param>
        /// <param name="startReadAddress">Address from where the data read begins.</param>
        /// <param name="numInputs">Length of data.</param>
        /// <param name="startWriteAddress">Address to where the data is written.</param>
        /// <param name="values">Contains the register information.</param>
        public void ReadWriteMultipleRegister(ushort id, byte unit, ushort startReadAddress, ushort numInputs,
            ushort startWriteAddress, byte[] values)
        {
            ushort numBytes = Convert.ToUInt16(values.Length);
            if (numBytes % 2 > 0) numBytes++;
            byte[] data;

            data = CreateReadWriteHeader(id, unit, startReadAddress, numInputs, startWriteAddress,
                Convert.ToUInt16(numBytes / 2));
            Array.Copy(values, 0, data, 17, values.Length);
            WriteAsyncData(data, id);
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

        /// <summary>
        ///  Create modbus header for read action
        /// </summary>
        /// <param name="id"></param>
        /// <param name="unit"></param>
        /// <param name="startAddress"></param>
        /// <param name="length"></param>
        /// <param name="function"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Create modbus header for write action
        /// </summary>
        /// <param name="id"></param>
        /// <param name="unit"></param>
        /// <param name="startAddress"></param>
        /// <param name="numData"></param>
        /// <param name="numBytes"></param>
        /// <param name="function"></param>
        /// <returns></returns>
        private byte[] CreateWriteHeader(ushort id, byte unit, ushort startAddress, ushort numData, ushort numBytes, byte function)
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

        /// <summary>
        /// Create modbus header for read/write action
        /// </summary>
        /// <param name="id"></param>
        /// <param name="unit"></param>
        /// <param name="startReadAddress"></param>
        /// <param name="numRead"></param>
        /// <param name="startWriteAddress"></param>
        /// <param name="numWrite"></param>
        /// <returns></returns>
        private byte[] CreateReadWriteHeader(ushort id, byte unit, ushort startReadAddress, ushort numRead, ushort startWriteAddress, ushort numWrite)
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

        /// <summary>
        /// Write asynchronous data
        /// </summary>
        /// <param name="write_data"></param>
        /// <param name="id"></param>
        private void WriteAsyncData(byte[] write_data, ushort id)
        {
            if ((_tcpAsyCl != null) && (_tcpAsyCl.Connected))
            {
                try
                {
                    _tcpAsyCl.BeginSend(write_data, 0, write_data.Length, SocketFlags.None, OnSend, null);
                    _tcpAsyCl.BeginReceive(_tcpAsyClBuffer, 0, _tcpAsyClBuffer.Length, SocketFlags.None, OnReceive,
                        _tcpAsyCl);
                }
                catch (SystemException)
                {
                    CallException(id, write_data[6], write_data[7], excExceptionConnectionLost);
                }
            }
            else CallException(id, write_data[6], write_data[7], excExceptionConnectionLost);
        }

        /// <summary>
        /// Write asynchronous data acknowledge
        /// </summary>
        /// <param name="result"></param>
        private void OnSend(IAsyncResult result)
        {
            if (result.IsCompleted == false) CallException(0xFFFF, 0xFF, 0xFF, excSendFailt);
        }

        /// <summary>
        /// Write asynchronous data response
        /// </summary>
        /// <param name="result"></param>
        private void OnReceive(IAsyncResult result)
        {
            if (result.IsCompleted == false) CallException(0xFF, 0xFF, 0xFF, excExceptionConnectionLost);

            ushort id = SwapUInt16(BitConverter.ToUInt16(_tcpAsyClBuffer, 0));
            byte unit = _tcpAsyClBuffer[6];
            byte function = _tcpAsyClBuffer[7];
            byte[] data;

            // Write response data
            if ((function >= fctWriteSingleCoil) && (function != fctReadWriteMultipleRegister))
            {
                data = new byte[2];
                Array.Copy(_tcpAsyClBuffer, 10, data, 0, 2);
            }
            else
            {
                // Read response data
                data = new byte[_tcpAsyClBuffer[8]];
                Array.Copy(_tcpAsyClBuffer, 9, data, 0, _tcpAsyClBuffer[8]);
            }

            // Response data is slave exception
            if (function > excExceptionOffset)
            {
                function -= excExceptionOffset;
                CallException(id, unit, function, _tcpAsyClBuffer[8]);
            }
            // Response data is regular data
            else if (OnResponseData != null) OnResponseData(id, unit, function, data);
        }

        /// <summary>
        /// Write data and and wait for response
        /// </summary>
        /// <param name="write_data"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        private byte[] WriteSyncData(byte[] write_data, ushort id)
        {
            if (_tcpSynCl.Connected) // todo добавить проверку на NULL (!!)
            {
                try
                {
                    _tcpSynCl.Send(write_data, 0, write_data.Length, SocketFlags.None);
                    int result = _tcpSynCl.Receive(_tcpSynClBuffer, 0, _tcpSynClBuffer.Length, SocketFlags.None);

                    byte unit = _tcpSynClBuffer[6];
                    byte function = _tcpSynClBuffer[7];
                    byte[] data;

                    if (result == 0) CallException(id, unit, write_data[7], excExceptionConnectionLost);

                    // ------------------------------------------------------------
                    // Response data is slave exception
                    if (function > excExceptionOffset)
                    {
                        function -= excExceptionOffset;
                        CallException(id, unit, function, _tcpSynClBuffer[8]);
                        return null;
                    }
                    // ------------------------------------------------------------
                    // Write response data
                    if ((function >= fctWriteSingleCoil) && (function != fctReadWriteMultipleRegister))
                    {
                        data = new byte[2];
                        Array.Copy(_tcpSynClBuffer, 10, data, 0, 2);
                    }
                    // ------------------------------------------------------------
                    // Read response data
                    else
                    {
                        data = new byte[_tcpSynClBuffer[8]];
                        Array.Copy(_tcpSynClBuffer, 9, data, 0, _tcpSynClBuffer[8]);
                    }
                    return data;
                }
                catch (SystemException)
                {
                    CallException(id, write_data[6], write_data[7], excExceptionConnectionLost);
                }
            }
            else CallException(id, write_data[6], write_data[7], excExceptionConnectionLost);
            return null;
        }

        #region IDisposable Members

        /// <summary>Destroy master instance</summary>
        public void Dispose() // todo без объявления IDisposable??
        {
            if (_tcpAsyCl != null)
            {
                if (_tcpAsyCl.Connected)
                {
                    try
                    {
                        _tcpAsyCl.Shutdown(SocketShutdown.Both);
                    }
                    catch
                    {
                    }
                    _tcpAsyCl.Close();
                }
                _tcpAsyCl = null;
            }
            if (_tcpSynCl != null)
            {
                if (_tcpSynCl.Connected)
                {
                    try
                    {
                        _tcpSynCl.Shutdown(SocketShutdown.Both);
                    }
                    catch
                    {
                    }
                    _tcpSynCl.Close();
                }
                _tcpSynCl = null;
            }
            _connected = false;
        }

        #endregion IDisposable Members

        /// <summary>Destroy master instance.</summary>
        ~Master()
        {
            Dispose();
        }
    }
}
