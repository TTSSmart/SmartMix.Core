using SmartMix.Core.Infrastructure.Plc.Client;
using SmartMix.Core.Infrastructure.Plc.Helpers;
using System.Diagnostics;
using static SmartMix.Core.Infrastructure.Plc.Interfaces.Delegates;

namespace SmartMix.Core.Infrastructure.Plc.PlcData
{
    /// <summary>
    /// Память PLC
    /// </summary>
    internal class PlcData : IDisposable
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
        /// Изменилось состояние соединения с PLC
        /// </summary>
        /// <value>
        /// Значение <see langword="true"/>, если соединение разорвано, 
        /// значение <see langword="false"/>, если соединение успешно установлено.
        /// </value>
        public event BoolEvent LostConn;

        #endregion События

        /// <summary>
        /// Представляет последнее состояние соединения с сервером.
        /// </summary>
        /// <value>
        /// Значение <see langword="true"/>, если соединение разорвано, 
        /// значение <see langword="false"/>, если соединение успешно установлено.
        /// </value>
        private bool _disconnected = false;

        private readonly Dictionary<int, PlcWord> _plcMemory;

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

        private readonly ushort _startNciAddr;
        private readonly ushort _minAddr;
        private readonly ushort _maxAddr;
        private bool _readAllData;

        /// <summary>
        /// IP PLC
        /// </summary>
        private string _plcIp;

        /// <summary>
        /// Порт
        /// </summary>
        private int _plcPort;

        private const ushort MODBUS_ID = 5;
        private readonly byte MODBUS_UNIT = 1;

        /// <summary>
        /// Представляет мастер драйвер Modbus TCP.
        /// </summary>
        private Master _mbm;

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

        /// <summary>Количество попыток чтения регистров до разрыва соединения </summary>
        private const int MAX_COUNT_REPEAT = 3;

        public PlcData(string plcIp, int plcPort, ushort minAdr, ushort maxAddr, ushort startNciAddr, byte modbusUnit = 1, int blockSize = 122)
        {
            MODBUS_UNIT = modbusUnit;

            _plcMemory = new Dictionary<int, PlcWord>();
            for (ushort addr = minAdr; addr <= maxAddr; addr++)
            {
                _plcMemory.Add(addr, new PlcWord(addr));
            }

            _minAddr = minAdr;
            _maxAddr = maxAddr;
            _startNciAddr = startNciAddr;

            _registersReadRanges = new MemoryRange[]
            {
                new MemoryRange() {StartAddress = _minAddr, EndAddress = _maxAddr, BlockSize = blockSize},
                new MemoryRange() {StartAddress = _minAddr, EndAddress = _startNciAddr, BlockSize = blockSize},
            };

            _writeQueue = new Queue<PlcWriteData>();
            _writeBoolData = new Dictionary<ushort, Dictionary<byte, bool>>();

            _pollingThread = new Thread(OnPollingHandler) { IsBackground = true };
            _watchDog = new Stopwatch();

            InitDriver(plcIp, plcPort);
        }

        public LogMessage ErrorLog { get; set; }

        /// <summary>
        /// Возвращает статус соединения.
        /// </summary>
        /// <value>Значение <see langword="true"/>, если соединение установлено, иначе - значение <see langword="false"/>.</value>
        public bool MBConnected => _mbm != null && _mbm.Connected;

        /// <summary>
        /// Получить значения
        /// </summary>
        /// <param name="addr">Первый адрес</param>
        /// <param name="size">Количество адресов</param>
        /// <returns></returns>
        public ushort[] GetData(ushort addr, byte size)
        {
            ushort[] res = new ushort[size];
            for (ushort i = 0; i < size; i++)
            {
                if (_plcMemory.ContainsKey(addr + i))
                    res[i] = _plcMemory[addr + i].Value;
            }
            return res;
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
            _onPolling = true;
            _pollingThread.Start();

        }
        public void SetReadAllData()
        {
            _readAllData = true;
        }

        public void Dispose()
        {
            ErrorLog = null;
            _onPolling = false; // сброс
            _pollingThread.Abort();

            _mbm.Disconnect();
            _mbm.Dispose();

            _writeQueue.Clear();
            _writeBoolData.Clear();
            _plcMemory.Clear();
        }

        /// <summary>
        /// Выполняет опрос регистров контроллера в отдельном потоке.
        /// </summary>
        private void OnPollingHandler()
        {
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
                            ErrorLog?.Invoke($"При работе с регистрами в потоке возникло исключение: {e.Message}");
                        }
                    }
                    else
                    {
                        // пробуем восстановить соединение
                        _disconnected = !TryConnect();
                        LostConn?.Invoke(_disconnected); // если контроллер сразу не подключен, то из-за более поздней загрузки клиентского приложения мы не видим об отсутствии связи на экране

                        if (_disconnected) Thread.Sleep(1000);  // таймаут простоя
                    }

                    _watchDog.Stop();
                    int sleep = 100 - (int)_watchDog.ElapsedMilliseconds;
                    if (sleep > 0) Thread.Sleep(sleep);
                }
                catch (Exception e)
                {
                    ErrorLog?.Invoke($"Возникла непредвиденная ошибка во время опроса регистров: {e.GetBaseException().Message}");
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
                        needReread = ReadData(readRange.StartAddress, readRange.EndAddress, readRange.BlockSize); // инициализация
                }

                if (!needReread && readRange.EndAddress == _maxAddr)
                    _readAllData = false;

                if (_disconnected != needReread)
                {
                    _disconnected = needReread;
                    LostConn?.BeginInvoke(_disconnected, null, null);
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
                    ReadBlockOnPlc(curaddr, step);
                    curaddr += step;
                }

                step = endAddr - curaddr + 1;
                if (step > 0)
                    ReadBlockOnPlc(curaddr, step);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void ReadBlockOnPlc(int stAddr, int count)
        {
            byte[] buffer = new byte[count * 2];
            _mbm.ReadHoldingRegister(MODBUS_ID, MODBUS_UNIT, (ushort)stAddr, (ushort)count, ref buffer);

            ushort[] regs_array = PlcIOHelper.ConvertBytesToUshorts(buffer);
            if (regs_array != null)
            {
                PlcWord word;
                for (int k = 0; k < regs_array.Length; k++)
                {
                    if (_plcMemory.TryGetValue(stAddr + k, out word))
                    {
                        word.Value = regs_array[k];
                    }
                }
            }
        }

        private void WriteData()
        {
            var task = Task<Queue<PlcWriteData>>.Factory.StartNew(GetBitRegsData);

            // константа 30 подобрана эмпирически
            // В этот блок попадает переключение тренажера(в тч запрос настроек при перезагрузке PLC)
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

            task.Wait();
            var queue = task.Result;

            while (queue.Count > 0)
            {
                WriteData(queue.Dequeue());
            }
        }

        private void WriteData(PlcWriteData data)
        {
            byte[] error_tmp = new byte[1];

            ushort[] values = data.Value;
            byte[] buffer = PlcIOHelper.ConvertUshortToBytes(values);

            if (values.Length == 1)
                _mbm.WriteSingleRegister(MODBUS_ID, MODBUS_UNIT, data.Address, buffer, ref error_tmp);
            else
                _mbm.WriteMultipleRegister(MODBUS_ID, MODBUS_UNIT, data.Address, buffer, ref error_tmp);

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
            List<PlcWriteData> sourceList = sourceQueue.ToList();
            sourceList.Sort((p1, p2) => { return p1.Address - p2.Address; });

            var outQueue = new Queue<PlcWriteData>();

            List<ushort> block;
            ushort address;

            while (sourceList.Count > 0)
            {
                block = new List<ushort>();

                var first = sourceList[0];
                sourceList.Remove(first);

                address = first.Address;
                block.AddRange(first.Value);

                /// * (first.Address + first.Values.Length == sourceList[0].Address) - проверка, что последняя добавленная и планируемая к добавлению переменная 
                ///следуют в памяти ПЛК строго друг за другом. 
                /// * (block.Count + sourceList[0].Values.Length < 122) - проверка на переполнение блока. Блок нельзя формировать больше 122 адресов.
                ///Связанно с ограничениями на размер пакета передаваемому по протоколу ModBUS
                while (sourceList.Count > 0 && first.Address + first.Value.Length == sourceList[0].Address && block.Count + sourceList[0].Value.Length < 122)
                {
                    first = sourceList[0];
                    sourceList.Remove(first);

                    block.AddRange(first.Value);
                    if (sourceList.Count == 0)
                        break;
                }

                outQueue.Enqueue(new PlcWriteData(address, block.ToArray()));
            }

            return outQueue;
        }

        private Queue<PlcWriteData> GetBitRegsData()
        {
            Queue<PlcWriteData> res = new Queue<PlcWriteData>();
            foreach (var keyValue in _writeBoolData)
            {
                var source = _plcMemory[keyValue.Key].Value;


                var result = source;
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

        /// <summary>
        /// Инициализировать ModbusTCP
        /// </summary>
        /// <param name="IPadr">IP адрес</param>
        /// <param name="port">Порт TCP</param>
        private void InitDriver(string IPadr, int port)
        {
            try
            {
                _plcIp = IPadr;
                _plcPort = port;
                _mbm = new Master();
                _mbm.Connect(IPadr, (ushort)port);
                var tmp = new byte[2];
                _mbm.ReadHoldingRegister(MODBUS_ID, MODBUS_UNIT, 0, 1, ref tmp);
            }
            catch (Exception e)
            {
                ErrorLog?.Invoke($"Произошла ошибка во время инициализации ModbusTCP: {e}");
            }
        }

        private bool TryConnect()
        {
            try
            {
                _mbm.Connect(_plcIp, (ushort)_plcPort);
                return _mbm.Connected;
            }
            catch (Exception e)
            {
                ErrorLog?.Invoke($"Произошла ошибка при установке подключения к ModbusTCP: {e}");
                return false;
            }
        }
    }
}
