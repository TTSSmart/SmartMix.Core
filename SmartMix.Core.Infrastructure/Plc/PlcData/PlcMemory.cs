using SmartMix.Core.Infrastructure.Plc.Client;
using SmartMix.Core.Infrastructure.Plc.Extensions;
using System.Diagnostics;
using static SmartMix.Core.Infrastructure.Plc.Interfaces.Delegates;

namespace SmartMix.Core.Infrastructure.Plc.PlcData
{
    /// <summary>
    /// Память PLC
    /// </summary>
    internal class PlcMemory : IDisposable
    {
        #region События

        /// <summary>
        /// Данные обновлены
        /// </summary>
        public event VoidEvent RegistersUpdated;

        /// <summary>
        /// Данные записаны
        /// </summary>
        public event VoidEvent RegistersWrited;

        /// <summary>
        /// Изменилось состояние соединения с PLC.
        /// В параметрах передаётся признак разрыва соединения.
        /// </summary>
        public event BoolEvent ConnectionChanged;

        #endregion События

        private readonly byte[] _memoryRaw = new byte[ushort.MaxValue];

        /// <summary>
        /// Очередь записи ячеек памяти
        /// </summary>
        private Queue<PlcWriteData> _writeQueue;

        /// <summary>
        /// Объект блокировки изменения очередей записи основных переменных
        /// </summary>
        private readonly object _writeLockObj = new object();

        /// <summary>
        /// Очередь записи bool и array переменных
        /// </summary>
        private readonly Dictionary<ushort, Dictionary<byte, bool>> _writeBoolData;

        /// <summary>
        /// Начальный адрес опроса регистров nci.
        /// </summary>
        private readonly ushort _startNciAddr;

        /// <summary>
        /// Начальный адрес опроса регистров.
        /// </summary>
        private readonly ushort _minAddr;

        /// <summary>
        /// Конечный адрес опроса регистров.
        /// </summary>
        private readonly ushort _maxAddr;
        private bool _readAllData;

        private const ushort MODBUS_ID = 5;
        private readonly byte MODBUS_UNIT = 1;

        /// <summary>
        /// Драйвер контроллера PLC
        /// </summary>
        private IModbusTcpClient _mbm;

        /// <summary>
        /// Поток опроса регистров.
        /// </summary>
        private readonly Thread _pollingThread;

        /// <summary>
        /// Признак операции опроса регистров.
        /// </summary>
        private bool _onPolling;

        /// <summary>
        /// Замер времени цикла: запись - чтение
        /// </summary>
        private Stopwatch _watchDog;

        /// <summary>
        /// Диапазоны чтения регистров
        /// 0 - все регистры
        /// 1 - все, кроме nci
        /// </summary>
        private MemoryRange[] _registersReadRanges;

        /// <summary>Количество попыток чтения регистров до разрыва соединения</summary>
        private const int MAX_COUNT_REPEAT = 3;

        /// <summary>
        /// Представляет признак потери связи с контроллером PLC.
        /// </summary>
        private volatile bool _disconnected = false;

        /// <summary>
        /// Представляет последнее состояние признака необходимости перечитывать регистры в случае ошибки чтения.
        /// </summary>
        private bool _lastRereadStatus = false;

        /// <summary>
        /// Делегат логирования ошибок.
        /// </summary>
        private LogMessage _errorLog;

        /// <summary>
        /// Инициализирует новый экземпляр класса по указанным параметрам.
        /// </summary>
        /// <param name="plcIp">IP-адрес подключения к контроллеру.</param>
        /// <param name="plcPort">Порт подключения к контроллеру.</param>
        /// <param name="minAddr">Начальный адрес опроса регистров.</param>
        /// <param name="maxAddr">Конечный адрес опроса регистров.</param>
        /// <param name="startNciAddr"></param>
        /// <param name="modbusUnit"></param>
        /// <param name="blockSize"></param>
        /// <param name="errorDelegate">Делегат логирования ошибок.</param>
        public PlcMemory(string plcIp, int plcPort, ushort minAddr, ushort maxAddr, ushort startNciAddr, byte modbusUnit = 1, int blockSize = 122, LogMessage errorDelegate = null)
        {
            MODBUS_UNIT = modbusUnit;

            _minAddr = minAddr;
            _maxAddr = maxAddr;
            _startNciAddr = startNciAddr;

            _registersReadRanges = new MemoryRange[]
            {
                new MemoryRange() {StartAddress = _minAddr, EndAddress = _maxAddr, BlockSize = blockSize},
                new MemoryRange() {StartAddress = _minAddr, EndAddress = _startNciAddr, BlockSize = blockSize},
            };

            _writeQueue = new Queue<PlcWriteData>();
            _writeBoolData = new Dictionary<ushort, Dictionary<byte, bool>>();
            _watchDog = new Stopwatch();

            _errorLog = errorDelegate;

            InitDriver(plcIp, plcPort);
            _pollingThread = new Thread(OnPollingHandler) { IsBackground = true };
        }

        /// <summary>
        /// Возвращает статус соединения с драйвером ModbusTCP.
        /// </summary>
        /// <returns>Значение <see langword="true"/>, если соединение установлено, иначе - значение <see langword="false"/>.</returns>
        public bool MBConnected => _mbm != null && _mbm.Connected;

        public int TimeUpdateRegs { get; private set; }

        /// <summary>
        /// Получить значения
        /// </summary>
        /// <param name="address">Первый адрес</param>
        /// <param name="size">Количество адресов</param>
        /// <returns></returns>        
        public byte[] GetDataNew(ushort address, byte size)
        {
            byte[] result = new byte[size * 2];
            if (address > 0)
            {
                return GetData(result, address, size);
            }
            return result;
        }

        private byte[] GetData(byte[] result, ushort address, byte size)
        {
            int offset = address - _minAddr;
            int startIndex = offset * 2;
            Array.Copy(_memoryRaw, startIndex, result, 0, size * 2);
            return result;
        }

        /// <summary>
        /// Записать значения
        /// </summary>
        /// <param name="addr">первый адрес</param>
        /// <param name="value">Данные</param>
        public void SetData(ushort addr, ushort[] value)
        {
            lock (_writeLockObj)
            {
                _writeQueue.Enqueue(new PlcWriteData(addr, value));
            }
        }
        /// <summary>
        /// Записать бит
        /// </summary>
        /// <param name="addr">Адрес</param>
        /// <param name="bitNum">Номер бита [0..15]</param>
        /// <param name="value">Значение</param>
        /// <exception cref="ArgumentException">"Если bitNum не принадлежит [0..15]"</exception>
        public void SetBit(ushort addr, byte bitNum, bool value)
        {
            if (bitNum < 0 || bitNum > 15) throw new ArgumentException("Некорректное значение номера бита", nameof(bitNum));

            lock (_writeLockObj)
            {
                if (_writeBoolData.TryGetValue(addr, out Dictionary<byte, bool> t))
                {
                    if (t.TryGetValue(bitNum, out bool val))
                    {
                        t[bitNum] = value;
                    }
                    else
                    {
                        t.Add(bitNum, value);
                    }
                }
                else
                {
                    var bytes = new Dictionary<byte, bool>();
                    bytes.Add(bitNum, value);
                    _writeBoolData.Add(addr, bytes);
                }
            }
        }

        /// <summary>
        /// Запустить поток чтения регистров. 
        /// </summary>
        public void StartPolling()
        {
            _readAllData = true;
            _onPolling = true; // инициализация
            _pollingThread.Start();
        }

        public void SetReadAllData()
        {
            _readAllData = true;
        }

        /// <summary>
        /// Выполняет опрос регистров контроллера в отдельном потоке.
        /// </summary>
        private void OnPollingHandler()
        {
            int sleep;
            while (_onPolling)
            {
                try
                {
                    _watchDog.Restart();
                    if (MBConnected)
                    {
                        try
                        {
                            lock (_writeLockObj)
                            {
                                WriteData();
                                RegistersWrited?.BeginInvoke(ar => RegistersWrited.EndInvoke(ar), null);
                            }

                            if (_readAllData)
                                ReadRegs(_registersReadRanges[0]);
                            else
                                ReadRegs(_registersReadRanges[1]);

                            RegistersUpdated?.BeginInvoke(ar => RegistersUpdated.EndInvoke(ar), null);
                        }
                        catch (Exception e)
                        {
                            _mbm.Disconnect();
                            _errorLog?.Invoke($"При работе с регистрами в потоке возникло исключение: {e.GetBaseException().Message}");
                        }
                    }
                    else
                    {
                        // пробуем восстановить соединение
                        _disconnected = !TryConnect();
                        ConnectionChanged?.Invoke(_disconnected);// если контроллер сразу не подключен, то из-за более поздней загрузки клиентского приложения мы не видим об отсутствии связи на экране

                        if (_disconnected) Thread.Sleep(1000); // таймаут простоя
                    }
                    _watchDog.Stop();
                    TimeUpdateRegs = (int)_watchDog.ElapsedMilliseconds;

                    sleep = 100 - TimeUpdateRegs;
                    if (sleep > 0) Thread.Sleep(sleep);
                }
                catch (Exception e)
                {
                    _errorLog?.Invoke($"Возникла непредвиденная ошибка во время опроса регистров: {e.GetBaseException().Message}");
                }
            }
        }

        /// <summary>
        /// Перечитывание значения регистров PLC.
        /// </summary>
        /// <param name="readRange">Диапазон регистров PLC.</param>
        private void ReadRegs(MemoryRange readRange)
        {
            if (MBConnected)
            {
                int iCount = 1;

                bool needReread = !ReadData(readRange.StartAddress, readRange.EndAddress, readRange.BlockSize); // инициализация : признак необходимости повторного чтения регистров в случае ошибки чтения
                while (needReread && iCount < MAX_COUNT_REPEAT)
                {
                    iCount++;
                    _mbm.Disconnect();

                    Thread.Sleep(500);

                    if (TryConnect())
                        needReread = !ReadData(readRange.StartAddress, readRange.EndAddress, readRange.BlockSize); // инициализация
                }

                if (!needReread && readRange.EndAddress == _maxAddr)
                    _readAllData = false;

                if (_lastRereadStatus != needReread)
                {
                    _lastRereadStatus = needReread;
                    ConnectionChanged?.BeginInvoke(_lastRereadStatus, null, null);
                }
            }
        }

        /// <summary>
        /// Выполняет считывание регистров PLC по указанным параметрам.
        /// Возвращает результат выполнения операции.
        /// </summary>
        /// <param name="startAddr">Первый адрес регистра.</param>
        /// <param name="endAddr">Последний адрес регистра.</param>
        /// <param name="step">Шаг</param>
        /// <returns>
        /// Значение <see langword="true"/>, если операция чтения прошло без ошибок,
        /// значение <see langword="false"/>, если во время операции возникло исключение.
        /// </returns>
        private bool ReadData(int startAddr, int endAddr, int step)
        {
            int curaddr = startAddr;
            try
            {
                while (curaddr + step < endAddr)
                {
                    ReadBlockInRaw(curaddr, step);
                    curaddr += step;
                }

                step = endAddr - curaddr + 1;
                if (step > 0)
                    ReadBlockInRaw(curaddr, step);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void ReadBlockInRaw(int startAddress, int count)
        {
            byte[] response = new byte[count * 2];
            _mbm.ReadHoldingRegister(MODBUS_ID, MODBUS_UNIT, (ushort)startAddress, (ushort)count, ref response);

            for (int k = 0; k < response.Length; k++)
            {
                int index = (startAddress - _minAddr) * 2 + k;
                if (_memoryRaw[index] != response[k])
                {
                    _memoryRaw[index] = response[k];
                }
            }
        }

        private void WriteData()
        {
            WriteQueueData();
            var queue = GetBitRegsData();
            while (queue.Count > 0)
            {
                WriteData(queue.Dequeue());
            }
        }

        private void WriteQueueData()
        {
            // константа 30 подобрана эмпирически
            // В этот блок попадает переключение тренажера(в т.ч. запрос настроек при перезагрузке PLC)
            // и отправка замеса. Блоки памяти записываемые в этих случаях находятся рядом. 
            // Поэтому имеет место существенный выигрыш по времени выполнения, несмотря на дополнительную процедуру сортировки
            // (от 10 раз для отправки замеса и до 100 раз при запросе настроек)
            if (_writeQueue.Count > 30)
            {
                var combine = CombineBlockWithSort(_writeQueue);

                while (combine.Count > 0)
                {
                    WriteData(combine.Dequeue());
                }
                _writeQueue.Clear();
            }
            else
            {
                while (_writeQueue.Count > 0)
                {
                    WriteData(_writeQueue.Dequeue());
                }
            }
        }

        private void WriteData(PlcWriteData data)
        {
            if (data.Address > 0)
            {
                WriteDataPlc(data);
            }
        }

        private void WriteDataPlc(PlcWriteData data)
        {
            byte[] request;
            byte[] error_tmp = new byte[1];

            ushort[] values = data.Value;
            if (values.Length == 1)
            {
                request = values.ToArrayBytes();
                _mbm.WriteSingleRegister(MODBUS_ID, MODBUS_UNIT, data.Address, request, ref error_tmp);
            }
            else
            {
                request = values.ToArrayBytes();
                _mbm.WriteMultipleRegister(MODBUS_ID, MODBUS_UNIT, data.Address, request, ref error_tmp);
            }

            if (data.Address >= _startNciAddr)
                _readAllData = true;
        }

        /// <summary>
        /// Производит сортировку по адресам и компоновку в блоки. Исходная очередь не изменяется.
        /// </summary>
        /// <param name="sourceQueue">очередь на запись</param>
        /// <returns>перекомпонованная очередь</returns>
        private Queue<PlcWriteData> CombineBlockWithSort(Queue<PlcWriteData> sourceQueue)
        {
            List<PlcWriteData> sourse = sourceQueue.ToList();
            sourse.Sort((p1, p2) => { return p1.Address - p2.Address; });

            var outQueue = new Queue<PlcWriteData>();

            List<ushort> block;
            ushort address;

            while (sourse.Count > 0)
            {
                block = new List<ushort>();

                var first = sourse[0];
                sourse.Remove(first);

                address = first.Address;
                block.AddRange(first.Value);

                /// * (first.Address + first.Values.Length == sourse[0].Address) - проверка, что последняя добавленная и планируемая к добавлению переменная 
                ///следуют в памяти ПЛК строго друг за другом. 
                /// * (block.Count + sourse[0].Values.Length < 122) - проверка на переполнение блока. Блок нельзя формировать больше 122 адресов.
                ///Связанно с ограничениями на размер пакета передаваемому по протоколу ModBUS
                while (sourse.Count > 0 && first.Address + first.Value.Length == sourse[0].Address && block.Count + sourse[0].Value.Length < 122)
                {
                    first = sourse[0];
                    sourse.Remove(first);

                    block.AddRange(first.Value);
                    if (sourse.Count == 0)
                        break;
                }

                outQueue.Enqueue(new PlcWriteData(address, block.ToArray()));
            }

            return outQueue;
        }

        private Queue<PlcWriteData> GetBitRegsData()
        {
            Queue<PlcWriteData> res = new Queue<PlcWriteData>();
            // bool-регистры должны записываться с конца так как по nviBATCH_ADD_ANS запускается заявка
            // и все bool-регистры после нее не попадут в заявку например nviLastBatch
            foreach (var keyValue in _writeBoolData.OrderByDescending(x => x.Key).AsEnumerable())
            {
                ushort source = GetCellArray(keyValue.Key);

                ushort result = source;
                foreach (var item in keyValue.Value)
                {
                    var mask = (1 << item.Key);

                    if (item.Value)
                    {
                        result = (ushort)(result | (mask));
                    }
                    else
                    {
                        result = (ushort)(result & ~(mask));
                    }
                }
                if (result != source)
                    res.Enqueue(new PlcWriteData(keyValue.Key, new[] { result }));
            }

            _writeBoolData.Clear();
            return res;
        }

        private ushort GetCellArray(int address)
        {
            int index = (address - _minAddr) * 2;
            byte[] value = new byte[2];
            value[0] = _memoryRaw[index + 1];
            value[1] = _memoryRaw[index];
            ushort result = BitConverter.ToUInt16(value, 0);
            return result;
        }

        /// <summary>
        /// Инициализировать драйвер ModbusTCP
        /// </summary>
        /// <param name="host">IP адрес</param>
        /// <param name="port">Порт</param>
        private void InitDriver(string host, int port)
        {
            try
            {
#if PLC_MOCK
                _mbm = new ModbusTcpClientMock(host, port);
#else
                _mbm = new ModbusTcpClient(host, port);
#endif
                _mbm.OnException += ModbusOnException;
                _mbm.OnResponse += ModbusReadOnResponse;
                if (!_mbm.TryConnect())
                    _errorLog?.Invoke($"Не удалось установить соединение с контролером {host}:{port}");

                //var tmp = new byte[2];
                //mbm.ReadHoldingRegister(MODBUS_ID, MODBUS_UNIT, 0, 1, ref tmp);
            }
            catch (Exception e)
            {
                _errorLog?.Invoke($"Произошла ошибка во время инициализации драйвера ModbusTCP: {e.GetBaseException().Message}");
            }
        }

        /// <summary>
        /// Выполняет попытку подключения к контроллеру. Возвращает результат выполнения операции.
        /// </summary>
        /// <returns>Значение <see langword="true"/>, если связь с контроллером ПЛК успешно установлена, иначе -значение  <see langword="false"/>.</returns>
        private bool TryConnect()
        {
            try
            {
                return _mbm.TryConnect();
            }
            catch (Exception e)
            {
                _errorLog?.Invoke($"Произошла ошибка во время подключения к драйверу ModbusTCP: {e.GetBaseException().Message}");
                return false;
            }
        }

        private void ModbusReadOnResponse(object sender, ModbusClientResponse e)
        {
            if (_disconnected)
            {
                _disconnected = false; // сбрасываем при получении входящего пакета
                ConnectionChanged?.Invoke(_disconnected);
            }
        }

        /// <summary>
        /// Обработка ошибок драйвера ModbusTCP
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ModbusOnException(object sender, ModbusClientException e)
        {
            if (!_disconnected)
            {
                _disconnected = true; // инициализация
                _errorLog?.Invoke($"Ошибка драйвера ModbusTCP: {e.Message}");
            }

            ConnectionChanged?.Invoke(_disconnected);
        }

        #region IDisposable Members

        private bool _disposed;

        private void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                // Dispose managed resources
                try
                {
                    //ErrorLog = null; // сбрасываем логирование #Ольга это должен делать owner
                    _onPolling = false; // сброс

                    try
                    {
                        _pollingThread?.Abort();
                    }
                    catch (Exception)
                    {
                        // ignore
                    }

                    _mbm?.Dispose();

                    _writeQueue.Clear();
                    _writeBoolData.Clear();
                }
                catch (Exception)
                {
                    // ignore
                }
            }

            // Free native resources
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Members

        ~PlcMemory()
        {
            Dispose(false);
        }
    }
}
