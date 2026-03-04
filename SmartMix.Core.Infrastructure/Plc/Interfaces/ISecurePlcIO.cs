using static SmartMix.Core.Infrastructure.Plc.Interfaces.Delegates;

namespace SmartMix.Core.Infrastructure.Plc.Interfaces
{
    /// <summary>
    /// Расширяет интерфейс <see cref="IPlcIO"/>
    /// <para>Предоставляет интерфейс для управления доступом к PLC.</para>
    /// <para>Должно производиться сравнение кода объекта на PLC и процедура регистрации сервера.</para>
    /// <para>Запись на PLC разрешена только если код объекта совпадает и сервер корректно зарегистрировался</para>
    /// </summary>
    public interface ISecurePlcIO : IPlcIO
    {
        /// <summary>
        /// Возвращает состояние "PLC занят/свободен".
        /// </summary>
        bool IsPlcBusy { get; }

        /// <summary>
        /// Событие проверки на соответствие кода объекта на линии на PLC, при этом отправляется
        /// значение <see langword="true"/>, если проверка выполнена успешно и коды проектов на клиенте и на сервере совпали, иначе - значение <see langword="false"/>.
        /// </summary>
        event BoolEvent LineIdentifierMatch;

        /// <summary>Событие изменение состояния "Контроллер занят другим сервером/Контроллер свободен".</summary>
        event BoolEvent PlcIsBusyState;

        /// <summary> Сервер начал регистрацию на PLC </summary>
        event VoidEvent ServerRegisterInit;

        /// <summary> Сервер прошел успешную регистрацию на PLC </summary>
        event VoidEvent ServerRegisterSuccess;

        /// <summary> Регистрация сервера на PLC прошла с ошибкой </summary>
        event LogMessage ServerRegisterError;
    }
}
