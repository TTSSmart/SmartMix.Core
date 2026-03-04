namespace SmartMix.Core.Infrastructure.Plc.Enums
{
    /// <summary>
    /// Указывает на состояние контроллера PLC
    /// </summary>
    internal enum PlcStatus
    {
        /// <summary>
        /// Свободен, или контроллер ещё не зарегистрирован
        /// </summary>
        Free,

        /// <summary>
        /// Контроллер с указанным ID зарегистрирован 
        /// </summary>
        /// <remarks>Доступ на запись разрешён</remarks>
        Access,

        /// <summary>
        /// Нет доступа на запись
        /// </summary>
        NoAccess
    }
}
