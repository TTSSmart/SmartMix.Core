using SmartMix.Core.Infrastructure.Plc.Enums;
using SmartMix.Core.Infrastructure.Plc.Helpers;
using SmartMix.Core.Infrastructure.Plc.Interfaces;
using SmartMix.Core.Infrastructure.Plc.Models;
using System.Runtime.CompilerServices;
using System.Text;
using static SmartMix.Core.Infrastructure.Plc.Interfaces.Delegates;

namespace SmartMix.Core.Infrastructure.Plc.Variables
{
    public class Variables : IPlcIO
    {
        private Dictionary<string, BoolVariable> _boolVariables;
        private Dictionary<string, IntVariable> _intVariables;
        private Dictionary<string, UIntVariable> _uIntVariables;
        private Dictionary<string, FloatVariable> _floatVariables;
        private Dictionary<string, ArrayVariable> _arrayVariables;

        private PlcData.PlcData _plcData;
        private bool _flushState;

        private object _lockUpdObj = new object();
        private bool _updIsWork;

        /// <summary>
        /// Событие обновления формы
        /// </summary>
        public event VoidEvent RegistersUpdated;

        /// <summary>
        /// Представляет событие изменения состояния соединения с PLC, при этом передаётся
        /// значение <see langword="true"/>, если соединение разорвано,
        /// или значение <see langword="false"/>, если соединение успешно установлено.
        /// </summary>
        public event BoolEvent LostConnection;

        private LogMessage _errLog;
        private LogMessage _infoLog;

        /// <summary>
        /// Представляет адрес контроллера в формате IP:PORT
        /// </summary>
        private readonly string _addressPlc;

        /// <summary>
        /// Задача для вызова события "Регистры обновлены"
        /// </summary>
        private Task _taskUpdatedRegs = Task.Factory.StartNew(() => { });

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address">IP-адрес контроллера</param>
        /// <param name="port">Номер порта контроллера</param>
        /// <param name="regList"></param>
        /// <param name="errorDelegate"></param>
        /// <param name="infoDelegate"></param>
        public Variables(string address, int port, string regList, LogMessage errorDelegate = null, LogMessage infoDelegate = null)
        {
            _addressPlc = address + ":" + port;
            _boolVariables = new Dictionary<string, BoolVariable>();
            _intVariables = new Dictionary<string, IntVariable>();
            _uIntVariables = new Dictionary<string, UIntVariable>();
            _floatVariables = new Dictionary<string, FloatVariable>();
            _arrayVariables = new Dictionary<string, ArrayVariable>();

            _errLog = errorDelegate;
            _infoLog = infoDelegate;

            var addrs = ReadFromCSVFile(regList, 12288);

            _plcData = new PlcData.PlcData(address, port, addrs.Item1, addrs.Item2, addrs.Item3);

            _plcData.ErrorLog = (error) => { _errLog?.Invoke($"_plcData: {error}"); };
            _plcData.RegistersUpdated += UpdateVariables;
            _plcData.RegistersWrited += PlcData_RegistersWrited;
            _plcData.LostConn += PlcData_LostConn;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address">IP-адрес контроллера</param>
        /// <param name="port">Номер порта контроллера</param>
        /// <param name="regsList"></param>
        /// <param name="modBusNumber"></param>
        public Variables(string address, int port, string regsList, byte modBusNumber)
        {
            _boolVariables = new Dictionary<string, BoolVariable>();
            _intVariables = new Dictionary<string, IntVariable>();
            _uIntVariables = new Dictionary<string, UIntVariable>();
            _floatVariables = new Dictionary<string, FloatVariable>();
            _arrayVariables = new Dictionary<string, ArrayVariable>();

            Tuple<ushort, ushort, ushort> addrs = ReadFromCSVFile(regsList, 0);

            _plcData = new PlcData.PlcData(address, port, addrs.Item1, addrs.Item2, addrs.Item3, modBusNumber);

            _plcData.ErrorLog = (error) => { _errLog?.Invoke($"_plcData: {error}"); };
            _plcData.RegistersUpdated += UpdateVariables;
            _plcData.RegistersWrited += PlcData_RegistersWrited;
            _plcData.LostConn += PlcData_LostConn;
        }

        public bool IsPlcActive => _plcData.MBConnected;

        #region Чтение переменных
        private void WriteInfoLog(string name, string typeName) => _infoLog?.Invoke($"Переменная {name} ({typeName}) не найдена. Создана заглушка.");
        public BoolVariable GetBoolVariable(string name)
        {
            if (!_boolVariables.ContainsKey(name))
            {
                _boolVariables.Add(name, new BoolVariable(name, 0, 1, VariableAccessLevel.Read));
                WriteInfoLog(name, nameof(BoolVariable));
            }
            return _boolVariables[name];
        }
        public IntVariable GetIntVariable(string name)
        {
            if (!_intVariables.ContainsKey(name))
            {
                _intVariables.Add(name, new IntVariable(name, 0, VariableAccessLevel.Read));
                WriteInfoLog(name, nameof(IntVariable));
            }
            return _intVariables[name];
        }
        public UIntVariable GetUIntVariable(string name)
        {
            if (!_uIntVariables.ContainsKey(name))
            {
                _uIntVariables.Add(name, new UIntVariable(name, 0, VariableAccessLevel.Read));
                WriteInfoLog(name, nameof(UIntVariable));
            }
            return _uIntVariables[name];
        }
        public FloatVariable GetFloatVariable(string name)
        {
            if (!_floatVariables.ContainsKey(name))
            {
                _floatVariables.Add(name, new FloatVariable(name, 0, VariableAccessLevel.Read));
                WriteInfoLog(name, nameof(FloatVariable));
            }
            return _floatVariables[name];
        }
        public ArrayVariable GetArrayVariable(string name)
        {
            if (!_arrayVariables.ContainsKey(name))
            {
                _arrayVariables.Add(name, new ArrayVariable(name, 0, 0xFF, VariableAccessLevel.Read));
                WriteInfoLog(name, nameof(ArrayVariable));
            }
            return _arrayVariables[name];
        }

        public BoolVariable[] GetAllBoolVariable()
        {
            return _boolVariables.Select(var => var.Value).ToArray();
        }
        public IntVariable[] GetAllIntVariable()
        {
            return _intVariables.Select(var => var.Value).ToArray();
        }
        public UIntVariable[] GetAllUIntVariable()
        {
            return _uIntVariables.Select(var => var.Value).ToArray();
        }
        public FloatVariable[] GetAllFloatVariable()
        {
            return _floatVariables.Select(var => var.Value).ToArray();
        }
        public ArrayVariable[] GetAllArrayVariable()
        {
            return _arrayVariables.Select(var => var.Value).ToArray();
        }

        public Variable GetVariableByName(string name)
        {
            if (_intVariables.ContainsKey(name))
            {
                return _intVariables[name].Clone();
            }
            else if (_uIntVariables.ContainsKey(name))
            {
                return _uIntVariables[name].Clone();
            }
            else if (_floatVariables.ContainsKey(name))
            {
                return _floatVariables[name].Clone();
            }
            else if (_boolVariables.ContainsKey(name))
            {
                return _boolVariables[name].Clone();
            }
            else if (_arrayVariables.ContainsKey(name))
            {
                return _arrayVariables[name].Clone();
            }
            else throw new ArgumentException($"Регистр {name} не найден.");
        }

        public Variable[] GetAllVariable()
        {
            List<Variable> vars = new List<Variable>();
            vars.AddRange(_boolVariables.Select(v => v.Value));
            vars.AddRange(_intVariables.Select(v => v.Value));
            vars.AddRange(_uIntVariables.Select(v => v.Value));
            vars.AddRange(_floatVariables.Select(v => v.Value));
            vars.AddRange(_arrayVariables.Select(v => v.Value));

            return vars.ToArray();
        }
        #endregion

        #region Запись переменных
        public WriteVariableResult WriteBitsInArray(string name, int[] bitNumbers, bool[] values)
        {
            if (_arrayVariables.TryGetValue(name, out ArrayVariable r))
            {
                if (!CheckWriteAccess(r)) return new WriteVariableResult(false, $"Регистр /{name}/ только для чтения");

                for (int i = 0; i < bitNumbers.Length; i++)
                {
                    var normBit = bitNumbers[i].Normalize(out int offset);
                    if (!CheckOffset(r, offset)) continue;

                    WriteBit((ushort)(r.Address + offset), (byte)normBit, values[i]);
                }
                return new WriteVariableResult(true);
            }
            return new WriteVariableResult(false, $"Регистр /{name}/ не найден в словаре array");
        }
        public WriteVariableResult WriteFullArrayRegVal(string name, ushort[] data)
        {
            if (_arrayVariables.TryGetValue(name, out ArrayVariable r))
            {
                if (!CheckWriteAccess(r)) return new WriteVariableResult(false, $"Регистр /{name}/ только для чтения");
                if (data.Length > r.Size) return new WriteVariableResult(false, $"Размер данных ({data.Length}) превышает размер регистра: ({name} - {r.Size})");

                WriteReg(r.Address, data);
                return new WriteVariableResult(true);
            }
            return new WriteVariableResult(false, $"Регистр /{name}/ не найден в словаре array");
        }
        /// <summary>
        /// Записать значение в регистр массив как импульс
        /// </summary>
        /// <param name="name">Имя</param>
        /// <param name="bitNum">Номер бита</param>
        /// <param name="value">Значение</param>
        /// <param name="time">Время</param>
        /// <returns></returns>
        public WriteVariableResult WriteBitInArrayReg(string name, int bitNum, bool value, int time)
        {
            if (_arrayVariables.TryGetValue(name, out ArrayVariable r))
            {
                if (!CheckWriteAccess(r)) return new WriteVariableResult(false, $"Регистр /{name}/ только для чтения");
                var normBit = bitNum.Normalize(out int offset);
                if (!CheckOffset(r, offset)) return new WriteVariableResult(false, $"В регистре /{name}/ нет {bitNum} бита");

                WriteBit((ushort)(r.Address + offset), (byte)normBit, value);
                Thread.Sleep(time);
                WriteBit((ushort)(r.Address + offset), (byte)normBit, !value);
                return new WriteVariableResult(true);
            }
            return new WriteVariableResult(false, $"Регистр /{name}/ не найден в словаре array");
        }
        public Task<WriteVariableResult> WriteBitInArrayRegAsync(string name, int bitNum, bool value, int time)
        {
            var task = new Task<WriteVariableResult>(() =>
            {
                if (_arrayVariables.TryGetValue(name, out ArrayVariable r))
                {
                    if (!CheckWriteAccess(r)) return new WriteVariableResult(false, $"Регистр /{name}/ только для чтения");
                    var normBit = bitNum.Normalize(out int offset);
                    if (!CheckOffset(r, offset)) return new WriteVariableResult(false, $"В регистре /{name}/ нет {bitNum} бита");

                    WriteBit((ushort)(r.Address + offset), (byte)normBit, value);
                    Thread.Sleep(time);
                    WriteBit((ushort)(r.Address + offset), (byte)normBit, !value);
                    return new WriteVariableResult(true);
                }
                return new WriteVariableResult(false, $"Регистр /{name}/ не найден в словаре array");
            });
            task.Start();
            return task;
        }
        /// <summary>
        /// Записать значение в регистр массив
        /// </summary>
        /// <param name="name">Имя</param>
        /// <param name="bitNum">Номер бита</param>
        /// <param name="value">Значение</param>
        /// <returns></returns>
        public WriteVariableResult WriteBitInArrayReg(string name, int bitNum, bool value)
        {
            if (_arrayVariables.TryGetValue(name, out ArrayVariable r))
            {
                if (!CheckWriteAccess(r)) return new WriteVariableResult(false, $"Регистр /{name}/ только для чтения");
                var normBit = bitNum.Normalize(out int offset);
                if (!CheckOffset(r, offset)) return new WriteVariableResult(false, $"В регистре /{name}/ нет {bitNum} бита");

                WriteBit((ushort)(r.Address + offset), (byte)normBit, value);
                return new WriteVariableResult(true);
            }
            return new WriteVariableResult(false, $"Регистр /{name}/ не найден в словаре array");
        }
        /// <summary>
        /// Запись bool регистра как импульс
        /// </summary>
        /// <param name="name">имя регистра</param>
        /// <param name="value">значение</param>
        /// <param name="time">Время импульса</param>
        /// <returns>true - успех; false - ошибка</returns>
        public WriteVariableResult WriteBoolRegVal(string name, bool value, int time)
        {
            if (_boolVariables.TryGetValue(name, out BoolVariable r))
            {
                if (!CheckWriteAccess(r)) return new WriteVariableResult(false, $"Регистр /{name}/ только для чтения");

                WriteBit(r.Address, r.BitNumber, value);
                Thread.Sleep(time);
                WriteBit(r.Address, r.BitNumber, !value);
                return new WriteVariableResult(true);
            }

            return new WriteVariableResult(false, $"Регистр /{name}/ не найден в словаре bool");
        }
        public WriteVariableResult WriteBoolRegVal(string name, bool value)
        {
            if (_boolVariables.TryGetValue(name, out BoolVariable r))
            {
                if (!CheckWriteAccess(r)) return new WriteVariableResult(false, $"Регистр /{name}/ только для чтения");

                WriteBit(r.Address, r.BitNumber, value);
                return new WriteVariableResult(true);
            }

            return new WriteVariableResult(false, $"Регистр /{name}/ не найден в словаре bool");
        }
        public WriteVariableResult WriteFloatRegVal(string name, float val)
        {
            if (_floatVariables.TryGetValue(name, out FloatVariable r))
            {
                if (!CheckWriteAccess(r)) return new WriteVariableResult(false, $"Регистр /{name}/ только для чтения");

                r.Value = val;
                WriteReg(r.Address, r.GetRawData());
                return new WriteVariableResult(true);
            }

            return new WriteVariableResult(false, $"Регистр /{name}/ не найден в словаре float");
        }
        public WriteVariableResult WriteIntRegVal(string name, int val)
        {
            if (_intVariables.TryGetValue(name, out IntVariable r))
            {
                if (!CheckWriteAccess(r)) return new WriteVariableResult(false, $"Регистр /{name}/ только для чтения");

                r.Value = val;
                WriteReg(r.Address, r.GetRawData());
                return new WriteVariableResult(true);
            }

            return new WriteVariableResult(false, $"Регистр /{name}/ не найден в словаре int");
        }
        public WriteVariableResult WriteUIntRegVal(string name, uint val)
        {
            if (_uIntVariables.TryGetValue(name, out UIntVariable r))
            {
                if (!CheckWriteAccess(r)) return new WriteVariableResult(false, $"Регистр /{name}/ только для чтения");

                r.Value = val;
                WriteReg(r.Address, r.GetRawData());
                return new WriteVariableResult(true);
            }

            return new WriteVariableResult(false, $"Регистр /{name}/ не найден в словаре uint");
        }
        [Obsolete]
        public WriteVariableResult WriteUnknownVariable(string name, uint value)
        {
            Variable var;
            try
            {
                var = SearchVariable(name);
            }
            catch (ArgumentException)
            {
                string ex = $"WriteUnknownVariable: Попытка записи регистра: {name} - {value}. Регистр не обнаружен";
                _errLog?.Invoke(ex);
                return new WriteVariableResult(false, ex);
            }

            if (!CheckWriteAccess(var)) return new WriteVariableResult(false, $"Регистр /{name}/ только для чтения");

            switch (var.Type)
            {
                case VariableType.Bool:
                    BoolVariable bVar = var as BoolVariable;
                    WriteBit(bVar.Address, bVar.BitNumber, value > 0);
                    break;

                case VariableType.Int:
                    IntVariable intVar = (var as IntVariable).Clone();
                    intVar.Value = (int)value;
                    WriteReg(intVar.Address, intVar.GetRawData());
                    break;

                case VariableType.Uint:
                    UIntVariable uintVar = (var as UIntVariable).Clone();
                    uintVar.Value = value;
                    WriteReg(uintVar.Address, uintVar.GetRawData());
                    break;

                case VariableType.Float:
                    FloatVariable floatVar = (var as FloatVariable).Clone();
                    floatVar.Value = value;
                    WriteReg(floatVar.Address, floatVar.GetRawData());
                    break;

                case VariableType.Array:
                    return new WriteVariableResult(false, $"Регистр /{name}/ является массивом. Запись не возможна");
            }
            return new WriteVariableResult(false, $"WriteUnknownVariable. Попытка записи регистра - /{name}/. Неизвестная ошибка.");
        }

        private Variable SearchVariable(string name)
        {
            if (_intVariables.ContainsKey(name))
            {
                return _intVariables[name];
            }
            else if (_uIntVariables.ContainsKey(name))
            {
                return _uIntVariables[name];
            }
            else if (_floatVariables.ContainsKey(name))
            {
                return _floatVariables[name];
            }
            else if (_boolVariables.ContainsKey(name))
            {
                return _boolVariables[name];
            }
            else if (_arrayVariables.ContainsKey(name))
            {
                return _arrayVariables[name];
            }
            else throw new ArgumentException($"Регистр {name} не найден");
        }

        /// <summary>
        /// Записать бит. Прямая запись без проверок
        /// </summary>
        /// <param name="adr">Адрес</param>
        /// <param name="bitNum">Номер бита</param>
        /// <param name="val">Значение</param>
        private void WriteBit(ushort adr, byte bitNum, bool val)
        {
            _plcData.SetBit(adr, bitNum, val);
        }
        /// <summary>
        /// Запись регистра(ов). Прямая запись без проверок
        /// </summary>
        /// <param name="adr">Начальный адрес</param>
        /// <param name="data">Массив регистров.</param>
        private void WriteReg(ushort adr, ushort[] data)
        {
            _plcData.SetData(adr, data);
        }

        /// <summary>
        /// Проверить доступность записи в переменную
        /// </summary>
        /// <param name="var"></param>
        /// <returns></returns>
        private bool CheckWriteAccess(Variable var)
        {
            return var.AccessLevel == VariableAccessLevel.ReadWrite;
        }

        /// <summary>
        /// Проверить смещение адреса для регистра массива
        /// </summary>
        /// <param name="var">Регистр массив</param>
        /// <param name="offset">Смещение</param>
        /// <returns></returns>
        private bool CheckOffset(ArrayVariable var, int offset)
        {
            return var.Size - 1 >= offset;
        }

        public void FlushWrite([CallerMemberName] string callMethod = null)
        {
            _flushState = true;
            while (_flushState) Thread.Sleep(100);
        }
        #endregion

        public void StartUpdate()
        {
            _plcData.StartPolling();
        }

        /// <summary>
        /// Разбор списка сетевых переменных
        /// </summary>
        /// <param name="regsList">Сетевые переменные</param>
        /// <returns>Item1 - startAddr; Item2 - endAddr; Item3 - firstNciAddr</returns>
        private Tuple<ushort, ushort, ushort> ReadFromCSVFile(string regsList, ushort start)
        {
            ushort startAddr = start;
            ushort endAddr = 0;
            ushort firstNciAddr = 0;
            #region Определение первого адреса регистра nci
            bool needFind = true; // флаг остановки поиска
            #endregion

            string line; //строка файла

            var ms = new MemoryStream(Encoding.ASCII.GetBytes(regsList));
            var sr = new StreamReader(ms);

            while (!sr.EndOfStream)
            {
                line = sr.ReadLine();
                try
                {
                    if (!line.Contains(";;"))
                    {
                        line = line.Trim();

                        string[] conf = line.Split(';');

                        if (conf.Length < 2) continue;

                        string[] type_array = null;
                        if (conf[0].Contains(":"))
                        {
                            type_array = conf[0].Split(':');
                        }

                        //Чтение типа
                        VariableType type;
                        byte array_size = 0;
                        if (type_array == null)
                        {
                            type = PlcIOHelper.GetEnum4Description<VariableType>(conf[0].Replace("\t", ""), '|');
                        }
                        else
                        {
                            type = PlcIOHelper.GetEnum4Description<VariableType>(type_array[0].Replace("\t", ""), '|');
                            array_size = byte.Parse(type_array[1]);
                        }

                        //Чтение имени
                        if (conf[1].Length == 0) continue;
                        string var_name = conf[1];

                        //Чтение адреса
                        UInt16 adr;
                        byte mask = 0;
                        if (conf[2].Contains("."))
                        {
                            string register_adr = null;
                            string bit_adr = null;
                            int j = 0;
                            while (conf[2][j] != '.') register_adr += conf[2][j++];
                            j++;
                            while (j < conf[2].Length) bit_adr += conf[2][j++];
                            adr = (UInt16)(int.Parse(register_adr));
                            mask = byte.Parse(bit_adr);
                        }
                        else
                        {
                            adr = (UInt16)(int.Parse(conf[2]));
                        }

                        #region Определение первого адреса регистра nci
                        if (needFind && var_name.Contains("nci"))
                        {
                            firstNciAddr = adr;
                            needFind = false;
                        }
                        #endregion

                        if (adr > endAddr)
                        {
                            endAddr = adr;
                            if (type == VariableType.Float) endAddr++;
                        }

                        VariableAccessLevel accessLevel = PlcIOHelper.GetEnum4Description<VariableAccessLevel>(conf[3]);
                        switch (type)
                        {
                            case VariableType.Bool:
                                _boolVariables.Add(var_name, new BoolVariable(var_name, adr, mask, accessLevel));
                                break;
                            case VariableType.Int:
                                _intVariables.Add(var_name, new IntVariable(var_name, adr, accessLevel));
                                break;
                            case VariableType.Uint:
                                _uIntVariables.Add(var_name, new UIntVariable(var_name, adr, accessLevel));
                                break;
                            case VariableType.Float:
                                _floatVariables.Add(var_name, new FloatVariable(var_name, adr, accessLevel));
                                break;
                            case VariableType.Array:
                                _arrayVariables.Add(var_name, new ArrayVariable(var_name, adr, array_size, accessLevel));
                                break;
                        }
                    }
                }
                catch (Exception e)
                {
                    _errLog?.Invoke($"[{_addressPlc}] При чтении файла регистров возникло исключение: {e.GetBaseException().Message}");
                }
            }

            _infoLog?.Invoke($"[{_addressPlc}] Чтение списка регистров произведено успешно. NCI start: {firstNciAddr}");
            return new Tuple<ushort, ushort, ushort>(startAddr, endAddr, firstNciAddr);
        }

        /// <summary>
        /// Обработчик изменения статусе соединения с контроллером.
        /// </summary>
        /// <param name="disconnected">
        /// Значение <see langword="true"/>, если соединение разорвано, 
        /// значение <see langword="false"/>, если соединение успешно установлено.
        /// </param>
        private void PlcData_LostConn(bool disconnected)
        {
            if (disconnected)
                _errLog?.Invoke($"Связь с контроллером <{_addressPlc}> потеряна");
            else
                _infoLog?.Invoke($"Связь с контроллером <{_addressPlc}> восстановлена");
            LostConnection?.Invoke(disconnected);
        }

        private void PlcData_RegistersWrited()
        {
            _flushState = false;
        }

        private void UpdateVariables()
        {
            lock (_lockUpdObj)
            {
                if (_updIsWork) return;
                _updIsWork = true;
            }

            List<Task> tasks = new List<Task>();

            tasks.Add(Task.Factory.StartNew(() => ReadVars(_boolVariables.Select(var => var.Value).ToArray())));
            tasks.Add(Task.Factory.StartNew(() => ReadVars(_intVariables.Select(var => var.Value).ToArray())));
            tasks.Add(Task.Factory.StartNew(() => ReadVars(_uIntVariables.Select(var => var.Value).ToArray())));
            tasks.Add(Task.Factory.StartNew(() => ReadVars(_floatVariables.Select(var => var.Value).ToArray())));
            tasks.Add(Task.Factory.StartNew(() => ReadVars(_arrayVariables.Select(var => var.Value).ToArray())));

            Task.WaitAll(tasks.ToArray());

            if (_taskUpdatedRegs.IsCompleted && RegistersUpdated != null)
                _taskUpdatedRegs = Task.Factory.StartNew(RegistersUpdated.Invoke);

            if (GetBoolVariable(PlcVarsPatternsHelper.nviFLAG_UPDATE_NCI).Value)
            {
                _plcData.SetReadAllData();
                WriteBoolRegVal(PlcVarsPatternsHelper.nviFLAG_UPDATE_NCI, false);
            }

            lock (_lockUpdObj)
            {
                _updIsWork = false;
            }
        }

        private void ReadVars(Variable[] vars)
        {
            foreach (var var in vars)
            {
                ushort[] data = _plcData.GetData(var.Address, var.Size);
                var.SetRawData(data);
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void Subscribe<T>(string name, EventHandler<T> eventHandler)
        {
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void Unsubscribe<T>(string name, EventHandler<T> eventHandler)
        {
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
                    _plcData.RegistersUpdated -= UpdateVariables;
                    _plcData.LostConn -= PlcData_LostConn;
                    _plcData.Dispose();

                    _boolVariables.Clear();
                    _intVariables.Clear();
                    _uIntVariables.Clear();
                    _floatVariables.Clear();
                    _arrayVariables.Clear();
                }
                catch (Exception)
                {
                    // ignore
                }
            }

            // Free native resources
            _disposed = true;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Members

        ~Variables()
        {
            Dispose(false);
        }
    }
}
