using SmartMix.Core.Infrastructure.Plc.Models;
using SmartMix.Core.Infrastructure.Plc.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static SmartMix.Core.Infrastructure.Plc.Interfaces.Delegates;

namespace SmartMix.Core.Infrastructure.Plc.Interfaces
{
    public interface IPlcIO : IDisposable
    {
        /// <summary>Событие "Регистры обновлены".</summary>
        event VoidEvent RegistersUpdated;

        /// <summary>Событие "Потеряно соединение c PLC".</summary>
        event BoolEvent LostConnection;

        /// <summary>
        /// Подписка на изменения регистров
        /// </summary>
        void Subscribe<T>(string name, EventHandler<T> eventHandler);

        /// <summary>
        /// Отписка на изменения регистров
        /// </summary>
        void Unsubscribe<T>(string name, EventHandler<T> eventHandler);

        #region Чтение регистров

        /// <summary>Получить bool переменную по имени </summary>
        /// <param name="name">Имя переменной</param>
        /// <returns><see cref="BoolVariable"/></returns>
        /// <exception cref="ArgumentException">Переменная не найдена</exception>
        BoolVariable GetBoolVariable(string name);

        /// <summary>Получить int переменную по имени </summary>
        /// <param name="name">Имя переменной</param>
        /// <returns><see cref="IntVariable"/></returns>
        /// <exception cref="ArgumentException">Переменная не найдена</exception>
        IntVariable GetIntVariable(string name);

        /// <summary>Получить uint переменную по имени </summary>
        /// <param name="name">Имя переменной</param>
        /// <returns><see cref="UIntVariable"/></returns>
        /// <exception cref="ArgumentException">Переменная не найдена</exception>
        UIntVariable GetUIntVariable(string name);

        /// <summary>Получить float переменную по имени </summary>
        /// <param name="name">Имя переменной</param>
        /// <returns><see cref="FloatVariable"/></returns>
        /// <exception cref="ArgumentException">Переменная не найдена</exception>
        FloatVariable GetFloatVariable(string name);

        /// <summary>Получить array переменную по имени </summary>
        /// <param name="name">Имя переменной</param>
        /// <returns><see cref="ArrayVariable"/></returns>
        /// <exception cref="ArgumentException">Переменная не найдена</exception>
        ArrayVariable GetArrayVariable(string name);

        /// <summary>Получить Все доступные bool переменные</summary>
        /// <returns> Массив <see cref="BoolVariable"/></returns>
        /// <exception cref="ArgumentException">Переменная не найдена</exception>
        BoolVariable[] GetAllBoolVariable();

        /// <summary>Получить Все доступные int переменные</summary>
        /// <returns> Массив <see cref="IntVariable"/></returns>
        IntVariable[] GetAllIntVariable();

        /// <summary>Получить Все доступные uint переменные</summary>
        /// <returns> Массив <see cref="UIntVariable"/></returns>
        UIntVariable[] GetAllUIntVariable();

        /// <summary>Получить Все доступные float переменные</summary>
        /// <returns> Массив <see cref="FloatVariable"/></returns>
        FloatVariable[] GetAllFloatVariable();

        /// <summary>Получить Все доступные array переменные</summary>
        /// <returns> Массив <see cref="ArrayVariable"/></returns>
        ArrayVariable[] GetAllArrayVariable();

        /// <summary>Получить переменную неизвестного типа по имени</summary>
        /// <param name="name">Имя переменной</param>
        /// <returns><see cref="Variable"/></returns>
        Variable GetVariableByName(string name);

        /// <summary>Получить все переменные</summary>
        /// <returns>Массив <see cref="Variable"/></returns>
        Variable[] GetAllVariable();

        #endregion Чтение регистров

        #region Запись регистров
        WriteVariableResult WriteBitsInArray(string name, int[] bitNumbers, bool[] values);
        WriteVariableResult WriteFullArrayRegVal(string name, ushort[] data);
        /// <summary>
        /// Записать значение в регистр массив как импульс
        /// </summary>
        /// <param name="name">Имя</param>
        /// <param name="bitNum">Номер бита</param>
        /// <param name="value">Значение</param>
        /// <param name="time">Время</param>
        WriteVariableResult WriteBitInArrayReg(string name, int bitNum, bool value, int time);
        Task<WriteVariableResult> WriteBitInArrayRegAsync(string name, int bitNum, bool value, int time);

        /// <summary>
        /// Записать значение в регистр массив
        /// </summary>
        /// <param name="name">Имя</param>
        /// <param name="bitNum">Номер бита</param>
        /// <param name="value">Значение</param>
        WriteVariableResult WriteBitInArrayReg(string name, int bitNum, bool value);

        /// <summary>
        /// Запись bool регистра как импульс
        /// </summary>
        /// <param name="name">имя регистра</param>
        /// <param name="value">значение</param>
        /// <param name="time">Время импульса</param>
        WriteVariableResult WriteBoolRegVal(string name, bool value, int time);

        WriteVariableResult WriteBoolRegVal(string name, bool value);
        WriteVariableResult WriteFloatRegVal(string name, float val);
        WriteVariableResult WriteIntRegVal(string name, int val);
        WriteVariableResult WriteUIntRegVal(string name, uint val);

        [Obsolete]
        WriteVariableResult WriteUnknownVariable(string name, uint value);

        #endregion Запись регистров

        ///<summary>Признак "Подключение к PLC активно"</summary>
        ///<value>Состояние соединения с PLC</value>
        bool IsPlcActive { get; }

        /// <summary> Ожидание записи всех переменных </summary>
        /// <param name="logSource">Метод-источник</param>
        void FlushWrite([CallerMemberName] string logSource = null);

        /// <summary> Запуск обновления </summary>
        [Obsolete]
        void StartUpdate();
    }
}
