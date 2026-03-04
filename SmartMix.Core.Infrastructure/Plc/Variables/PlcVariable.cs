using SmartMix.Core.Infrastructure.Plc.Enums;
using SmartMix.Core.Infrastructure.Plc.Extensions;
using SmartMix.Core.Infrastructure.Plc.Helpers;
using SmartMix.Core.Infrastructure.Plc.Interfaces;
using SmartMix.Core.Infrastructure.Plc.Models;
using SmartMix.Core.Infrastructure.Plc.Parser;
using SmartMix.Core.Infrastructure.Plc.PlcData;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static SmartMix.Core.Infrastructure.Plc.Interfaces.Delegates;

namespace SmartMix.Core.Infrastructure.Plc.Variables
{
    public class PlcVariables : IPlcIO
    {
        private Dictionary<string, Variable> _variables = new Dictionary<string, Variable>();
        private readonly ConcurrentDictionary<string, Variable> _newVariables = new ConcurrentDictionary<string, Variable>();

        /// <summary>
        /// Представляет память PLC (?)
        /// </summary>
        private PlcMemory _plcMemory;

        /// <summary>
        /// Представляет предыдущее состояние связи с контроллером PLC.
        /// </summary>
        private volatile bool _lastDisconnected;

        private bool _isUpdateRegs;

        /// <summary>
        /// Представляет делегат логирования ошибок.
        /// </summary>
        private readonly LogMessage _errLog;

        /// <summary>
        /// Представляет делегат логирования отладочных сообщений.
        /// </summary>
        private readonly LogMessage _debugLog;

        /// <summary>
        /// Представляет делегат логирования информационных сообщений.
        /// </summary>
        private readonly LogMessage _infoLog;

        /// <summary>
        /// Представляет базовый адрес подключения к контроллеру PLC в формате ip:port.
        /// </summary>
        private readonly string _addressPlc;

        private bool _isStarted = false;

        private object _lockerOnRegistersUpdated = new object();
        private bool _onRegistersUpdated;

        /// <summary>
        /// Инициализирует новый экземпляр класса по указанным параметрам.
        /// </summary>
        /// <param name="address">IP-адрес контроллера PLC.</param>
        /// <param name="port">Порт подключения к контроллеру PLC.</param>
        /// <param name="regsList">Список сетевых переменных вида: (WORD|DWORD...);Имя;Адрес;(R|RW);[комментарий] ...</param>
        /// <param name="startAddress">Стартовый адрес опроса контроллера, где значение 16384 - контроллер CREVIS, значение 12288 - стандартный контроллер </param>
        /// <param name="errorDelegate">Делегат логирования ошибок.</param>
        /// <param name="infoDelegate">Делегат логирования информационных сообщений.</param>
        public PlcVariables(string address, int port, string regsList, int startAddress, LogMessage errorDelegate = null, LogMessage infoDelegate = null)
        {
            _addressPlc = $"{address}:{port}";

            _errLog = errorDelegate;
            _infoLog = infoDelegate;

#if LOGSUB
            _debugLog = errorDelegate;
#endif

            IParserResult<Variable> parsResult;
            if (startAddress == 12288)
                parsResult = new CsvRegisterParser().GetFromCsvText(regsList, (ushort)startAddress);
            else
                parsResult = new CrevisParser().GetFromCsvText(regsList, (ushort)startAddress);

            _variables = parsResult.Registers;

            _plcMemory = new PlcMemory(address, port, parsResult.StartAddress, parsResult.EndAddress, parsResult.FirstNciAddress, errorDelegate: errorDelegate);
            AddPlcHandlers();

            //UpdateWithoutNci = () => ReadVars(_variables.Values.Where(v => v.Address < parsResult.FirstNciAddress));
            //UpdateNci = () => ReadVars(_variables.Values.Where(v => v.Address >= parsResult.FirstNciAddress));
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса по указанным параметрам.
        /// </summary>
        /// <param name="address">IP-адрес контроллера PLC.</param>
        /// <param name="port">Порт подключения к контроллеру PLC.</param>
        /// <param name="regsList">Регистры</param>
        /// <param name="modBusNumber"></param>
        public PlcVariables(string address, int port, string regsList, byte modBusNumber)
        {
            IParserResult<Variable> parsResult = new CsvRegisterParser().GetFromCsvText(regsList, 0);
            _variables = parsResult.Registers;
            //_plcData = new PlcData.PlcData(address, port, addrs.Item1, addrs.Item2, addrs.Item3, modBusNumber);

            _plcMemory = new PlcMemory(address, port, parsResult.StartAddress, parsResult.EndAddress, parsResult.FirstNciAddress, modBusNumber);
            AddPlcHandlers();
        }

        #region IPlcIO Events

        /// <summary>
        /// Событие "Регистры обновлены".
        /// </summary>
        public event VoidEvent RegistersUpdated;

        private void OnRegistersUpdated() => RegistersUpdated?.Invoke();

        /// <summary>
        /// Событие изменения состояния соединения с контроллером PLC, при этом в значением параметра является признак разрыва соединения.
        /// </summary>
        public event BoolEvent LostConnection;

        /// <summary>
        /// Генерирует событие "Состояние соединения с контроллером PLC было изменено".
        /// </summary>
        /// <param name="disconnected">Признак разрыва соединения.</param>
        private void OnLostConnection(bool disconnected)
        {
            var handler = LostConnection;
            if (handler != null) handler(disconnected);
        }

        #endregion IPlcIO Events

        /// <summary>
        /// Возвращает статус соединения с контроллером.
        /// </summary>
        /// <returns>Значение <see langword="true"/>, если соединение установлено, иначе - значение <see langword="false"/>.</returns>
        public bool IsPlcActive => _plcMemory.MBConnected;

        /// <inheritdoc/>
        public void Subscribe<T>(string name, EventHandler<T> eventHandler)
        {
            if (_variables.TryGetValue(name, out Variable variable))
            {
                if (variable is IVariableValue<T> value)
                {
                    value.Changed += eventHandler;
                }
                else
                {
                    _debugLog?.Invoke($"Регистр {name} типа {typeof(T).Name} ожидался тип {variable.Type}");
                }
            }
            else
            {
                _debugLog?.Invoke($"Не удачная Подписка на регистр {name} типа {typeof(T).Name}");
            }
        }

        /// <inheritdoc/>
        public void Unsubscribe<T>(string name, EventHandler<T> eventHandler)
        {
            if (_variables.TryGetValue(name, out Variable variable))
            {
                if (variable is IVariableValue<T> value)
                {
                    value.Changed -= eventHandler;
                }
            }
        }

        /// <summary>
        /// Ожидание записи всех переменных
        /// </summary>
        /// <param name="logSource">Метод-источник</param>
        public void FlushWrite([CallerMemberName] string logSource = null)
        {
            do
            {
                if (_disposed) break;

                Thread.Sleep(_plcMemory.TimeUpdateRegs);

            } while (!_isUpdateRegs);

            _isUpdateRegs = false;
        }

        /// <summary>
        /// Запуск обновления.
        /// </summary>
        public void StartUpdate()
        {
            _isStarted = true;
            _plcMemory.StartPolling();
        }

        #region IPlcIO Чтение регистров
        public BoolVariable GetBoolVariable(string name)
        {
            if (_variables.TryGetValue(name, out Variable value))
                return value as BoolVariable;

            return CheckRunAndAddMock<BoolVariable>(name);
        }

        public IntVariable GetIntVariable(string name)
        {
            if (_variables.TryGetValue(name, out Variable value))
                return value as IntVariable;

            return CheckRunAndAddMock<IntVariable>(name);
        }
        public UIntVariable GetUIntVariable(string name)
        {
            if (_variables.TryGetValue(name, out Variable value))
                return value as UIntVariable;

            return CheckRunAndAddMock<UIntVariable>(name);
        }
        public FloatVariable GetFloatVariable(string name)
        {
            if (_variables.TryGetValue(name, out Variable value))
                return value as FloatVariable;

            return CheckRunAndAddMock<FloatVariable>(name);
        }
        public ArrayVariable GetArrayVariable(string name)
        {
            if (_variables.TryGetValue(name, out Variable value))
                return value as ArrayVariable;

            return CheckRunAndAddMock<ArrayVariable>(name);
        }

        public BoolVariable[] GetAllBoolVariable()
            => _variables.Values.Where(t => t is BoolVariable).Cast<BoolVariable>().ToArray();

        public IntVariable[] GetAllIntVariable()
            => _variables.Values.Where(t => t is IntVariable).Cast<IntVariable>().ToArray();

        public UIntVariable[] GetAllUIntVariable()
            => _variables.Values.Where(t => t is UIntVariable).Cast<UIntVariable>().ToArray();

        public FloatVariable[] GetAllFloatVariable()
            => _variables.Values.Where(t => t is FloatVariable).Cast<FloatVariable>().ToArray();

        public ArrayVariable[] GetAllArrayVariable()
            => _variables.Values.Where(t => t is ArrayVariable).Cast<ArrayVariable>().ToArray();

        public Variable GetVariableByName(string name)
        {
            if (_variables.TryGetValue(name, out Variable value))
                return value;

            throw new ArgumentException($"Регистр {name} не найден");
        }

        public Variable[] GetAllVariable()
        {
            return _variables.Values.ToArray();
        }

        #endregion IPlcIO Чтение регистров

        #region IPlcIO Запись в регистры
        public WriteVariableResult WriteBitsInArray(string name, int[] bitNumbers, bool[] values)
        {
            if (_variables.TryGetValue(name, out Variable r))
            {
                if (!CheckWriteAccess(r))
                    return ReadOnlyResult(name);

                for (int i = 0; i < bitNumbers.Length; i++)
                {
                    int normBit = bitNumbers[i].Int32Normalize(out int offset);

                    if (!CheckOffset((ArrayVariable)r, offset))
                        continue;

                    WriteBit(r, (ushort)(r.Address + offset), (byte)normBit, values[i]);
                }
                return new WriteVariableResult(true);
            }
            return NotFountResult(name);
        }

        public WriteVariableResult WriteFullArrayRegVal(string name, ushort[] data)
        {
            if (string.IsNullOrWhiteSpace(name))
                return NotFountResult(name);

            if (data == null)
                return new WriteVariableResult(false, nameof(ArgumentNullException));


            if (_variables.TryGetValue(name, out Variable r))
            {
                if (data.Length > r.Size) return new WriteVariableResult(false, $"Размер данных ({data.Length}) превышает размер регистра: ({name} - {r.Size})");
#if PLC_MOCK                
                r.SetRawData(data);
#else
                if (!CheckWriteAccess(r)) return ReadOnlyResult(name, false);
                WriteReg(r, data);
#endif
                return new WriteVariableResult(true);
            }
            return NotFountResult(name);
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
            if (_variables.TryGetValue(name, out Variable r))
            {
                if (!CheckWriteAccess(r)) return ReadOnlyResult(name, false);
                int normBit = bitNum.Int32Normalize(out int offset);
                if (!CheckOffset((ArrayVariable)r, offset)) return new WriteVariableResult(false, $"В регистре /{name}/ нет {bitNum} бита");

                WriteBit(r, (ushort)(r.Address + offset), (byte)normBit, value);
                Thread.Sleep(time);
                WriteBit(r, (ushort)(r.Address + offset), (byte)normBit, !value);
                return new WriteVariableResult(true);
            }
            return NotFountResult(name);
        }
        public async Task<WriteVariableResult> WriteBitInArrayRegAsync(string name, int bitNum, bool value, int time)
        {
            if (_variables.TryGetValue(name, out Variable r))
            {
#if PLC_MOCK
                ((ArrayVariable)r).Value.SetBitValue(bitNum, value);
                await Task.Delay(TimeSpan.FromMilliseconds(time));
                ((ArrayVariable)r).Value.SetBitValue(bitNum, !value);
#else
                if (!CheckWriteAccess(r)) return ReadOnlyResult(name);
                int normBit = bitNum.Int32Normalize(out int offset);
                if (!CheckOffset((ArrayVariable)r, offset)) return new WriteVariableResult(false, $"В регистре /{name}/ нет {bitNum} бита");

                WriteBit(r, (ushort)(r.Address + offset), (byte)normBit, value);
                await Task.Delay(TimeSpan.FromMilliseconds(time));
                WriteBit(r, (ushort)(r.Address + offset), (byte)normBit, !value);
#endif
                return new WriteVariableResult(true);
            }
            return NotFountResult(name);
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
            if (_variables.TryGetValue(name, out Variable r))
            {
#if PLC_MOCK
                BitArray newValue = ((ArrayVariable)r).Value;
                newValue.SetBitValue(bitNum, value);
                ((ArrayVariable)r).Value = newValue;
#else
                if (!CheckWriteAccess(r)) return ReadOnlyResult(name);
                int normBit = bitNum.Int32Normalize(out int offset);
                if (!CheckOffset((ArrayVariable)r, offset)) return new WriteVariableResult(false, $"В регистре /{name}/ нет {bitNum} бита");
                WriteBit(r, (ushort)(r.Address + offset), (byte)normBit, value);
#endif
                return new WriteVariableResult(true);
            }
            return NotFountResult(name);
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
            if (_variables.TryGetValue(name, out Variable r))
            {
                if (!CheckWriteAccess(r)) return ReadOnlyResult(name);
                byte bit = ((BoolVariable)r).BitNumber;
                WriteBit(r, r.Address, bit, value);
                Thread.Sleep(time);
                WriteBit(r, r.Address, bit, !value);
                return new WriteVariableResult(true);
            }

            return NotFountResult(name);
        }
        public WriteVariableResult WriteBoolRegVal(string name, bool value)
        {
            if (_variables.TryGetValue(name, out Variable r))
            {
                if (!CheckWriteAccess(r)) return ReadOnlyResult(name);
                byte bit = ((BoolVariable)r).BitNumber;
#if PLC_MOCK
                ((BoolVariable)r).Value = value;
#endif
                WriteBit(r, r.Address, bit, value);
                return new WriteVariableResult(true);
            }

            return NotFountResult(name);
        }

        public WriteVariableResult WriteFloatRegVal(string name, float val)
            => WriteRegValue<FloatVariable>(name, () => val.GetRaw(), (variable) => ((FloatVariable)variable).Value = val);

        public WriteVariableResult WriteIntRegVal(string name, int val)
            => WriteRegValue<IntVariable>(name, () => val.GetRaw(), (variable) => ((IntVariable)variable).Value = val);

        public WriteVariableResult WriteUIntRegVal(string name, uint val)
            => WriteRegValue<UIntVariable>(name, () => val.GetRaw(), (variable) => ((UIntVariable)variable).Value = val);

        private WriteVariableResult WriteRegValue<T>(string name, Func<ushort[]> converter, Action<Variable> action) where T : Variable
        {
            try
            {
                ushort[] newValue = converter.Invoke();
                Variable r = TryGetValue<T>(name, newValue);
#if !PLC_MOCK
                if (!CheckWriteAccess(r)) return ReadOnlyResult(name);
#else
                action.Invoke(r);
#endif
                WriteReg(r, newValue);
                return new WriteVariableResult(true);
            }
            catch (Exception ex)
            {
                _errLog?.Invoke(ex.Message);
                return NotFountResult(name);
            }
        }

        /// <summary>
        /// Попытка получить регистр, если его нет то создать
        /// </summary>
        private Variable TryGetValue<T>(string name, ushort[] value) where T : Variable
        {
            if (_variables.TryGetValue(name, out Variable r))
            {
                return r;
            }
            return CheckRunAndAddMock<T>(name, value);
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
                string error = $"[WriteUnknownVariable] Попытка записи регистра '{name}': {value} - регистр не обнаружен";
                _errLog?.Invoke(error);
                return new WriteVariableResult(false, error);
            }

            if (!CheckWriteAccess(var)) return ReadOnlyResult(name);

            switch (var.Type)
            {
                case VariableType.Bool:
                    BoolVariable bVar = var as BoolVariable;
                    WriteBit(bVar, bVar.Address, bVar.BitNumber, value > 0);
                    break;

                case VariableType.Int:
                    IntVariable intVar = (var as IntVariable).Clone();
                    intVar.Value = (int)value;
                    WriteReg(intVar, intVar.GetRawData());
                    break;

                case VariableType.Uint:
                    UIntVariable uintVar = (var as UIntVariable).Clone();
                    uintVar.Value = value;
                    WriteReg(uintVar, uintVar.GetRawData());
                    break;

                case VariableType.Float:
                    FloatVariable floatVar = (var as FloatVariable).Clone();
                    floatVar.Value = value;
                    WriteReg(floatVar, floatVar.GetRawData());
                    break;

                case VariableType.Array:
                    return new WriteVariableResult(false, $"Регистр /{name}/ является массивом. Запись не возможна");
            }
            return new WriteVariableResult(false, $"[WriteUnknownVariable] Попытка записи регистра '{name}': неизвестная ошибка");
        }

        private Variable SearchVariable(string name)
        {
            if (_variables.TryGetValue(name, out Variable value))
                return value;

            throw new ArgumentException($"Регистр {name} не найден");
        }

        /// <summary>
        /// Записать бит.
        /// </summary>
        /// <param name="variable">Регистр</param>
        /// <param name="bitNum">Номер бита</param>
        /// <param name="newValue">Значение</param>
        private void WriteBit(Variable variable, ushort address, byte bitNum, bool newValue)
        {
            if (address > 0)
            {
                WriteBitPlc(variable, address, bitNum, newValue);
            }
            else
            {
                WriteBitLocal(variable, address, bitNum, newValue);
            }
        }

        /// <summary>
        /// Запись регистра(ов).
        /// </summary>
        /// <param name="variable">Регистр</param>
        /// <param name="data">Массив регистров.</param>
        private void WriteReg(Variable variable, ushort[] data)
        {
            if (variable.Address > 0)
            {
                WritePlc(variable, data);
            }
            else
            {
                WriteLocal(variable, data);
            }
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

        #endregion IPlcIO Запись в регистры

        /// <summary>
        /// Обрабатывает изменение состояния подключения с контроллером PLC.
        /// </summary>
        /// <param name="disconnected">Признак разрыва соединения.</param>
        private void PlcData_ConnectionChanged(bool disconnected)
        {
            if (disconnected)
            {
                if (!_lastDisconnected) _errLog?.Invoke($"Связь с контроллером <{_addressPlc}> потеряна"); // чтобы не спамить лог
            }
            else
                _infoLog?.Invoke($"Связь с контроллером <{_addressPlc}> восстановлена");
            OnLostConnection(disconnected);

            _lastDisconnected = disconnected; // запомнили
        }

        /// <summary>
        /// 
        /// </summary>
        private void PlcData_RegistersWrited()
        {
            _isUpdateRegs = true;
        }

        /// <summary>
        /// 
        /// </summary>
        private void PlcData_RegistersUpdated()
        {
            try
            {
                lock (_lockerOnRegistersUpdated)
                {
                    if (_onRegistersUpdated) return;
                    _onRegistersUpdated = true;
                }

                ReadVars(_variables.Values);

                Task.Run(OnRegistersUpdated);

                if (GetBoolVariable(PlcVarsPatternsHelper.nviFLAG_UPDATE_NCI).Value)
                {
                    _plcMemory.SetReadAllData();
                    WriteBoolRegVal(PlcVarsPatternsHelper.nviFLAG_UPDATE_NCI, false);
                }

                lock (_lockerOnRegistersUpdated)
                    _onRegistersUpdated = false;
            }
            catch (Exception e)
            {
                _errLog?.Invoke($"[PlcData_RegistersUpdated] Произошла ошибка во время чтения регистров: {e}");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ReadVars(IEnumerable<Variable> vars)
        {
            if (_newVariables.Count > 0)
                AddNewVariable();

#if PLC_MOCK
            return;
#endif
            foreach (var v in vars)
            {
                if (v.Address > 0)
                {
                    v.Bytes = _plcMemory.GetDataNew(v.Address, v.Size);
                }
            }

        }

        private void AddNewVariable()
        {
#if DEBUG
            _infoLog?.Invoke($"Запись очереди [{_newVariables.Count}]");
#endif
            while (_newVariables.Count() > 0)
            {
                string key = _newVariables.FirstOrDefault().Key;
                if (_newVariables.TryRemove(key, out Variable variable))
                {
                    if (_variables.TryGetValue(key, out Variable _))
                        continue;

                    _variables.Add(key, variable);
#if DEBUG
                    _infoLog?.Invoke($"Добавлен [{variable.Name}]");
#endif
                }
            }
#if DEBUG
            _infoLog?.Invoke($"Запись очереди закончена [{_newVariables.Count}]");
#endif
        }

        private void WriteBitLocal(Variable variable, ushort address, byte bitNum, bool newValue)
        {
            if (_variables.TryGetValue(variable.Name, out Variable value))
            {
                System.Collections.BitArray bitArray = new System.Collections.BitArray(value.Bytes);
                bitArray.Set(bitNum, newValue);
                bitArray.CopyTo(value.Bytes, 0);
            }
        }

        private void WriteBitPlc(Variable variable, ushort address, byte bitNum, bool newValue)
            => _plcMemory.SetBit(address, bitNum, newValue);

        private void WriteLocal(Variable variable, ushort[] newValue)
        {
            if (_variables.TryGetValue(variable.Name, out Variable value))
            {
                value.SetRawData(newValue);
            }
        }

        private void WritePlc(Variable variable, ushort[] newValue)
            => _plcMemory.SetData(variable.Address, newValue);


        private Dictionary<Type, Func<string, Variable>> _variableCreator = new Dictionary<Type, Func<string, Variable>>()
        {
            { typeof(IntVariable),      (name) => new IntVariable   (name, 0, VariableAccessLevel.ReadWrite) },
            { typeof(UIntVariable),     (name) => new UIntVariable  (name, 0, VariableAccessLevel.ReadWrite) },
            { typeof(FloatVariable),    (name) => new FloatVariable (name, 0, VariableAccessLevel.ReadWrite) },
            { typeof(BoolVariable),     (name) => new BoolVariable  (name, 0, 15, VariableAccessLevel.ReadWrite) },
            { typeof(ArrayVariable),    (name) => new ArrayVariable (name, 0, 0xFF, VariableAccessLevel.ReadWrite) },
        };

        private T CheckRunAndAddMock<T>(string name, ushort[] value = null) where T : Variable
        {
            Variable result = _variableCreator[typeof(T)].Invoke(name);

            if (_isStarted)
            {
                //lock (_lock)
                //{
                if (value != null)
                    result.SetRawData(value);

                if (_newVariables.TryGetValue(name, out Variable variable))
                {
                    if (value != null)
                        variable.SetRawData(value);
                }
                else
                {
                    _newVariables.TryAdd(name, result);
                }
                //}                
#if Ghost
                InfoLog($"Переменная '{name}' ({typeof(T).Name}) не найдена. Добавлена в очередь.");
#endif
            }
            else
            {
                _infoLog?.Invoke($"Запрос переменной '{name}': опрос регистров ещё не запущен или в процессе запуска.");
            }

            return (T)result;
        }

        private WriteVariableResult NotFountResult(string name, [CallerMemberName] string logSource = null)
        {
            string error = $"Регистр {name} не найден в словаре array";

            _infoLog?.Invoke($"[{logSource}] {error}");
            return new WriteVariableResult(false, error);
        }

        private WriteVariableResult ReadOnlyResult(string name, bool toLog = true, [CallerMemberName] string logSource = null)
        {
            string error = $"Регистр {name} только для чтения";

            if (toLog) _infoLog?.Invoke($"[{logSource}] {error}");
            return new WriteVariableResult(false, error);
        }

        /// <summary>
        /// Подписываемся на события драйвера контроллера PLC.
        /// </summary>
        private void AddPlcHandlers()
        {
            _lastDisconnected = false; // сбросили на всякий случай

            _plcMemory.RegistersUpdated += PlcData_RegistersUpdated;
            _plcMemory.RegistersWrited += PlcData_RegistersWrited;
            _plcMemory.ConnectionChanged += PlcData_ConnectionChanged;
        }

        /// <summary>
        /// Отписываемся от событий драйвера контроллера PLC.
        /// </summary>
        private void RemovePlcHandlers()
        {
            if (_plcMemory != null)
            {
                _plcMemory.RegistersUpdated -= PlcData_RegistersUpdated;
                _plcMemory.RegistersWrited -= PlcData_RegistersWrited;
                _plcMemory.ConnectionChanged -= PlcData_ConnectionChanged;

                _plcMemory.Dispose();
            }
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
                    RemovePlcHandlers();

                    _variables.Clear();
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

        ~PlcVariables()
        {
            Dispose(false);
        }
    }
}
