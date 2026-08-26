using SmartMix.Core.Infrastructure.Plc.Enums;
using SmartMix.Core.Infrastructure.Plc.Helpers;
using SmartMix.Core.Infrastructure.Plc.Interfaces;
using SmartMix.Core.Infrastructure.Plc.Models;
using SmartMix.Core.Infrastructure.Plc.Security;
using SmartMix.Core.Infrastructure.Plc.Variables;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using static SmartMix.Core.Infrastructure.Plc.Interfaces.Delegates;

namespace TTS.PlcIO
{
    /// <summary>
    /// Прокси класс для <see cref="Variables.Variables"/>. Реализует <see cref="ISecurePlcIO"/> 
    /// <para>Добавляет поддержку авторизации сервера для разрешения записи на PLC.</para>
    /// <para>Чтение доступно всем. Для записи в PLC сервер должен провести процедуру регистрации на PLC.</para>
    /// <para>Для не зарегистрированных серверов событие <see cref="RegistersUpdated"/> выдаваться не будет.</para>
    /// <para>Добавляет события состояния регистрации: 
    /// <para> - <see cref="ServerRegisterInit"/> - Инициализация </para>
    /// <para> - <see cref="ServerRegisterSuccess"/> - Успех</para>
    /// <para> - <see cref="ServerRegisterError"/> - Ошибка</para></para>
    /// <para>Для работы необходимы сетевые переменные из файла <see cref="PlcVarsPatternsHelper"/></para>
    /// </summary>
    public class BSUVariablesProxy : ISecurePlcIO
    {
        #region События

        /// <summary>Представляет событие "Регистры были изменены".</summary>
        public event VoidEvent RegistersUpdated;

        /// <summary>
        /// Генерирует событие "Регистры были изменены".
        /// </summary>
        private void OnRegistersUpdated()
        {
            RegistersUpdated?.Invoke();
        }

        /// <summary>
        /// Представляет событие изменения состояния соединения с PLC, при этом передаётся
        /// значение <see langword="true"/>, если соединение разорвано,
        /// или значение <see langword="false"/>, если соединение успешно установлено.
        /// </summary>
        public event BoolEvent LostConnection;

        /// <summary>
        /// Генерирует событие "Статус соединения с контроллером PLC бы изменён".
        /// </summary>
        /// <param name="disconnected">
        /// Значение <see langword="true"/>, если соединение разорвано,
        /// или значение <see langword="false"/>, если соединение успешно установлено.
        /// </param>
        private void OnLostConnection(bool disconnected)
        {
            var handler = LostConnection;
            if (handler != null) handler(disconnected);
        }

        /// <summary>Результат проверки кодов объекта на клиенте VS PLC</summary>
        public event BoolEvent LineIdentifierMatch;

        /// <summary>Изменено состояние контроллера PLC "Занят\Свободен"</summary>
        public event BoolEvent PlcIsBusyState;

        /// <summary> Начата процедура регистрации сервера </summary>
        public event VoidEvent ServerRegisterInit;

        /// <summary> Сервер зарегистрировался успешно</summary>
        public event VoidEvent ServerRegisterSuccess;

        /// <summary>Логер регистрации ошибок регистрации сервера</summary>
        public event LogMessage ServerRegisterError;

        #endregion События

        private const string _accessDenied = "Запись запрещена: сервер не прошел процедуру регистрации";

        private readonly PlcVariables _variables;

        /// <summary>Представляет идентификатор код объекта на линии.</summary>
        /// <example>190101(001|002)</example>
        private uint _codeObject;

        /// <summary> Идентификатор ПК </summary>
        private int _serverIdentifier;

        /// <summary> Запись запрещена. Выставляется в false после успешной регистрации сервера</summary>
        private bool _writeAccessDenied;

        /// <summary>
        /// Представляет задачу по обработке состояния соединения с сервером.
        /// </summary>
        private Task _disconnectHandler;

        private Queue<Action> _queueWrite = new Queue<Action>(short.MaxValue);

        private object _locker = new object();
        /// <summary>
        /// Представляет признак выполнения операции по обработке регистров.
        /// </summary>
        private bool _onUpdateRegs;

        /// <summary> Начата процедура регистрации сервера </summary>
        private bool _regStarted;
        private Waiter<bool> _waitNvoTransmission;

        /// <summary>
        /// Представляет делегат логирования всех ошибок, за исключением ошибок регистрации сервера..
        /// </summary>
        private readonly LogMessage _errLog;

        /// <summary>
        /// Прокси класс для <see cref="Variables.Variables"/>
        /// </summary>
        /// <param name="address">IP PLC</param>
        /// <param name="port">Port PLC</param>
        /// <param name="regsList">Список сетевых переменных вида: (WORD|DWORD...);Имя;Адрес;(R|RW);[комментарий] ...</param>
        /// <param name="startAddress">Стартовый адрес опроса контроллера.</param>
        /// <param name="errorDelegate">Делегат передачи сообщений об ошибках</param>
        /// <param name="infoDelegate">Делегат передачи информационных сообщений</param>
        public BSUVariablesProxy(string address, int port, string regsList, int startAddress, LogMessage errorDelegate = null, LogMessage infoDelegate = null)
        {
            _writeAccessDenied = true;

            _errLog = errorDelegate;

            _variables = new PlcVariables(address, port, regsList, startAddress, errorDelegate, infoDelegate);
            _variables.LostConnection += OnLostConnection; // todo где-то надо отписаться
            _variables.RegistersUpdated += Variables_RegistersUpdated;

            _waitNvoTransmission = new Waiter<bool>(() => _variables.GetBoolVariable(PlcVarsPatternsHelper.nvoTRANSMITTED).Value, 100);
            _serverIdentifier = (int)PCIdentifier.GetPCid();

            _disconnectHandler = DisconnectHandler();

            _ = WriteAccessDenied();
        }

        #region Properties

        /// <summary>Возвращает статус соединения с контроллеромPLC </summary>
        /// <value>Значение <see langword="true"/>, если соединение успешно установлено, иначе - значение <see langword="false"/>.</value>
        public bool IsPlcActive => _variables.IsPlcActive;

        /// <summary>Представляет признак "PLC занят: контроллер работает с другим сервером [запись запрещена]"</summary>
        private bool _plcIsBusy;

        /// <summary>Возвращает или задаёт признак "PLC занят: контроллер работает с другим сервером [запись запрещена]"</summary>
        public bool IsPlcBusy
        {
            get
            {
                return _plcIsBusy;
            }
            private set
            {
                //if (_plcIsBusy != value)  // где-то не проходит инициализация, и не восстанавливается состояние контроллера после его перезагрузки, поэтому убрали сравнение на изменение
                {
                    _plcIsBusy = value;
                    PlcIsBusyState?.BeginInvoke(_plcIsBusy, null, null);
                }
            }
        }

        #endregion Properties

        /// <summary>
        /// Обрабатывает изменение регистров.
        /// </summary>
        private async void Variables_RegistersUpdated()
        {
            lock (_locker)
            {
                if (_onUpdateRegs) return;

                _onUpdateRegs = true; // инициализация

                // предварит. проверка на совпадение кодов объекта
                bool match = AreProjectCodesMatch();
                LineIdentifierMatch?.BeginInvoke(match, null, null);

                if (!match)
                {
                    _onUpdateRegs = false;
                    return;
                }
            }

            try
            {
                PlcStatus status = CheckPlcStatus();

                _writeAccessDenied = status != PlcStatus.Access;
                switch (status)
                {
                    case PlcStatus.Free:
                        _regStarted = true; // инициализация
                        await RegisterServerOnPlc();
                        break;

                    case PlcStatus.Access:
                        if (_regStarted) ServerRegisterSuccess?.Invoke();

                        _regStarted = false; // сброс

                        IsPlcBusy = false;
                        OnRegistersUpdated();
                        break;

                    case PlcStatus.NoAccess:
                        IsPlcBusy = true;
                        break;
                }
            }
            catch (Exception e)
            {
                _errLog?.Invoke($"[Variables_RegistersUpdated] Произошла ошибка во время обработки регистров: {e}");
            }
            finally
            {
                lock (_locker)
                    _onUpdateRegs = false; // сброс
            }
        }

        /// <summary>
        /// Выполняет предварительную проверку на совпадение кодов объекта на клиенте и контроллере PLC. 
        /// Возвращает результат проверки.
        /// </summary>
        /// <returns>Значение <see langword="true"/>, если коды объектов были проверены и совпадают, иначе - значение <see langword="false"/>.</returns>
        private bool AreProjectCodesMatch()
        {
            try
            {
#if PLC_MOCK
            _variables.WriteUIntRegVal(PlcVarsPatternsHelper.nvoProject_ID, _codeObject);
            _variables.WriteIntRegVal(PlcVarsPatternsHelper.nviID_PC, _serverIdentifier);
            _variables.WriteIntRegVal(PlcVarsPatternsHelper.nvoID_PC, _serverIdentifier);
#endif
                int plcCode = (int)_variables.GetUIntVariable(PlcVarsPatternsHelper.nvoProject_ID).Value;

                bool checkResult = _codeObject == plcCode;

                if (!checkResult)
                    ServerRegisterError?.Invoke($"Не совпадают коды проектов SmartMix и PLC: {_codeObject} VS {plcCode}");

                return checkResult;
            }
            catch (Exception e)
            {
                _errLog?.Invoke($"[Variables_RegistersUpdated] Произошла ошибка во время проверки кода объекта: {e}");
                return false;
            }
        }

        /// <summary>
        /// Запрашивает статус контроллера PLC.
        /// </summary>
        /// <returns>Текущий статус контроллера PLC.</returns>
        private PlcStatus CheckPlcStatus()
        {
            int idPC = _variables.GetIntVariable(PlcVarsPatternsHelper.nvoID_PC).Value;
            if (idPC == 0) return PlcStatus.Free;

            if (idPC == _serverIdentifier) return PlcStatus.Access;
            return PlcStatus.NoAccess;
        }

        /// <summary>
        /// Регистрирует сервер на контроллере PLC.
        /// </summary>
        /// <returns></returns>
        private async Task RegisterServerOnPlc()
        {
            ServerRegisterInit?.BeginInvoke(null, null);
#if PLC_MOCK
            await Task.Delay(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
#else
            if (_variables.GetBoolVariable(PlcVarsPatternsHelper.nvoSN_NOT_CORRECT).Value)
                ServerRegisterError?.Invoke("nvoSN_NOT_CORRECT = true");

            _variables.WriteBoolRegVal(PlcVarsPatternsHelper.nviTRANSMITTED, false);

            if (await _waitNvoTransmission.Wait(false, 10))
            {
                _variables.WriteIntRegVal(PlcVarsPatternsHelper.nviID_PC, _serverIdentifier);
                _variables.WriteBoolRegVal(PlcVarsPatternsHelper.nviTRANSMITTED, true);

                if (await _waitNvoTransmission.Wait(true, 10))
                {
                    if (!_variables.GetBoolVariable(PlcVarsPatternsHelper.nvoLICENSE).Value)
                        ServerRegisterError?.Invoke("nvoLICENSE = false");
                }
                else
                {
                    ServerRegisterError?.Invoke("nvoTRANSMITTED не переключилось в true за 10 тактов");
                }
            }
            else
            {
                ServerRegisterError?.Invoke("nvoTRANSMITTED не переключилось в false за 10 тактов");
            }
#endif
        }

        /// <summary>
        /// Использовать перегрузку : <Code>StartUpdate(uint codeObject)</Code>
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        [Obsolete]
        public void StartUpdate()
        {
            if (_codeObject == 0)
                throw new ArgumentException("Не указан код объекта на клиенте", nameof(_codeObject));

            StartUpdate(_codeObject);
        }

        /// <summary>Запустить обновление</summary>
        /// <param name="codeObject">Код объекта: 190101(001|002)</param>
        public void StartUpdate(uint codeObject)
        {
            _codeObject = codeObject;
            _variables.StartUpdate();
        }

        #region Чтение регистров
        public ArrayVariable[] GetAllArrayVariable() => _variables.GetAllArrayVariable();
        public BoolVariable[] GetAllBoolVariable() => _variables.GetAllBoolVariable();
        public FloatVariable[] GetAllFloatVariable() => _variables.GetAllFloatVariable();
        public IntVariable[] GetAllIntVariable() => _variables.GetAllIntVariable();
        public UIntVariable[] GetAllUIntVariable() => _variables.GetAllUIntVariable();

        public Variable[] GetAllVariable() => _variables.GetAllVariable();

        public ArrayVariable GetArrayVariable(string name) => _variables.GetArrayVariable(name);
        public BoolVariable GetBoolVariable(string name) => _variables.GetBoolVariable(name);
        public FloatVariable GetFloatVariable(string name) => _variables.GetFloatVariable(name);
        public IntVariable GetIntVariable(string name) => _variables.GetIntVariable(name);
        public UIntVariable GetUIntVariable(string name) => _variables.GetUIntVariable(name);
        public Variable GetVariableByName(string name) => _variables.GetVariableByName(name);

        #endregion Чтение регистров

        #region Запись регистров
        public WriteVariableResult WriteBitInArrayReg(string name, int bitNum, bool value, int time)
        {
            if (_plcIsBusy) return new WriteVariableResult(false, "Контроллер занят.");
            if (_writeAccessDenied) return new WriteVariableResult(false, "Запись запрещена: сервер не прошел процедуру регистрации");

            return _variables.WriteBitInArrayReg(name, bitNum, value, time);
        }
        public WriteVariableResult WriteBitInArrayReg(string name, int bitNum, bool value)
        {
            if (_plcIsBusy) return new WriteVariableResult(false, "Контроллер занят.");
            if (_writeAccessDenied) return new WriteVariableResult(false, "Запись запрещена: сервер не прошел процедуру регистрации");

            return _variables.WriteBitInArrayReg(name, bitNum, value);
        }
        public Task<WriteVariableResult> WriteBitInArrayRegAsync(string name, int bitNum, bool value, int time)
        {
            if (_plcIsBusy) new Task<WriteVariableResult>(() => { return new WriteVariableResult(false, "Контроллер занят."); });
            if (_writeAccessDenied) new Task<WriteVariableResult>(() => { return new WriteVariableResult(false, "Запись запрещена: сервер не прошел процедуру регистрации"); });

            return _variables.WriteBitInArrayRegAsync(name, bitNum, value, time);
        }
        public WriteVariableResult WriteBitsInArray(string name, int[] bitNumbers, bool[] values)
        {
            if (_plcIsBusy) return new WriteVariableResult(false, "Контроллер занят.");
            if (_writeAccessDenied) return new WriteVariableResult(false, "Запись запрещена: сервер не прошел процедуру регистрации");

            return _variables.WriteBitsInArray(name, bitNumbers, values);
        }
        public WriteVariableResult WriteBoolRegVal(string name, bool value, int time)
        {
            if (_plcIsBusy) return new WriteVariableResult(false, "Контроллер занят.");
            if (_writeAccessDenied) return new WriteVariableResult(false, "Запись запрещена: сервер не прошел процедуру регистрации");

            return _variables.WriteBoolRegVal(name, value, time);
        }
        public WriteVariableResult WriteBoolRegVal(string name, bool value)
        {
            if (_plcIsBusy) return new WriteVariableResult(false, "Контроллер занят.");
            if (_writeAccessDenied) return new WriteVariableResult(false, "Запись запрещена. Сервер не прошел процедуру регистрации");

            return _variables.WriteBoolRegVal(name, value);
        }
        public WriteVariableResult WriteFloatRegVal(string name, float val)
        {
            if (_plcIsBusy) return new WriteVariableResult(false, "Контроллер занят.");
            if (_writeAccessDenied) return new WriteVariableResult(false, "Запись запрещена: сервер не прошел процедуру регистрации");

            return _variables.WriteFloatRegVal(name, val);
        }
        public WriteVariableResult WriteFullArrayRegVal(string name, ushort[] data)
        {
            if (_plcIsBusy) return new WriteVariableResult(false, "Контроллер занят.");
            if (_writeAccessDenied) return new WriteVariableResult(false, "Запись запрещена: сервер не прошел процедуру регистрации");

            return _variables.WriteFullArrayRegVal(name, data);
        }
        public WriteVariableResult WriteIntRegVal(string name, int val)
        {
            if (_plcIsBusy) return new WriteVariableResult(false, "Контроллер занят.");

            if (_writeAccessDenied) return WriteAccessDenied(name, () => Write(name, val, _variables.GetIntVariable, _variables.WriteIntRegVal));

            return _variables.WriteIntRegVal(name, val);
        }
        public WriteVariableResult WriteUIntRegVal(string name, uint val)
        {
            if (_plcIsBusy) return new WriteVariableResult(false, "Контроллер занят.");
            if (_writeAccessDenied) return new WriteVariableResult(false, "Запись запрещена: сервер не прошел процедуру регистрации");

            return _variables.WriteUIntRegVal(name, val);
        }

        [Obsolete]
        public WriteVariableResult WriteUnknownVariable(string name, uint value)
        {
            if (_plcIsBusy) return new WriteVariableResult(false, "Контроллер занят.");
            if (_writeAccessDenied) return new WriteVariableResult(false, "Запись запрещена: сервер не прошел процедуру регистрации");

            return _variables.WriteUnknownVariable(name, value);
        }

        private void Write<T, V>(string name, T value, Func<string, V> func, Func<string, T, WriteVariableResult> write)
            where V : Variable
        {
            V variable = func.Invoke(name);
            if (variable.Address > 0) return;

#if AccessDenied
            ServerRegisterError.Invoke($"[{name} = {value}]");
#endif
            write.Invoke(name, value);
        }

        private WriteVariableResult WriteAccessDenied(string name, Action action)
        {
            lock (_locker)
            {
                _queueWrite.Enqueue(action);

#if AccessDenied
                ServerRegisterError.Invoke($"[{name}] {AccessDenied}");
#endif
                return new WriteVariableResult(false, $"[{name}] {_accessDenied}");
            }
        }

        #endregion Запись регистров

        /// <summary>Ожидание записи всех регистров.</summary>
        /// <param name="logSource">Метод-источник</param>
        public void FlushWrite([CallerMemberName] string logSource = null)
        {
            _variables.FlushWrite(logSource);
        }

        /// <summary>
        /// Сервер говорит PLC что живой
        /// </summary>
        private async Task DisconnectHandler()
        {
            while (true) // todo нужен _cancelTockenSource
            {
                bool isConnect = _variables.GetBoolVariable(PlcVarsPatternsHelper.ServerDisconnected).Value;
                if (isConnect)
                    _variables.WriteBoolRegVal(PlcVarsPatternsHelper.ServerDisconnected, !isConnect);

                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }

        private async Task WriteAccessDenied()
        {
            while (_writeAccessDenied)
            {
                await Task.Delay(TimeSpan.FromSeconds(3));
            }

            lock (_locker)
            {
                foreach (Action action in _queueWrite)
                    action.Invoke();
            }
        }

        /// <inheritdoc/>
        public void Subscribe<T>(string name, EventHandler<T> eventHandler) => _variables.Subscribe(name, eventHandler);

        /// <inheritdoc/>
        public void Unsubscribe<T>(string name, EventHandler<T> eventHandler) => _variables.Unsubscribe(name, eventHandler);

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
                    _variables.WriteIntRegVal(PlcVarsPatternsHelper.nviID_PC, _serverIdentifier);
                    _variables.Dispose();
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

        ~BSUVariablesProxy()
        {
            Dispose(false);
        }
    }
}
