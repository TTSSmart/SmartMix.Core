namespace SmartMix.Core.Infrastructure.Plc.Helpers
{
    internal static class PlcVarsPatternsHelper
    {
        /// <summary>
        /// Флаг изменения переменных nci на контроллере. Необходимо обновить регистры на сервере.
        /// </summary>
        public const string nviFLAG_UPDATE_NCI = "nviFLAG_UPDATE_NCI";

        /// <summary>Код объекта, зарегистрированный на контроллере PLC</summary>
        /// <example>190101(001|002)</example>
        public const string nvoProject_ID = "nvoProject_ID";

        /// <summary>Идентификатор ПК-сервера, который зарегистрирован на PLC</summary>
        public const string nvoID_PC = "nvoID_PC";

        /// <summary>Идентификатор ПК-текущего сервера для регистрации</summary>
        public const string nviID_PC = "nviID_PC";

        /// <summary> Версия программы на PLC </summary>
        public const string nvoProject_Version = "nvoProject_Version";

        /// <summary>
        /// Ответ PLC на запрос начала процесса регистрации
        /// </summary>
        public const string nvoTRANSMITTED = "nvoTRANSMITTED";

        /// <summary>
        /// Триггер сервера на действия по регистрации сервера
        /// </summary>
        public const string nviTRANSMITTED = "nviTRANSMITTED";
        public const string nvoLICENSE = "nvoLICENSE";
        public const string nvoSN_NOT_CORRECT = "nvoSN_NOT_CORRECT";

        public const string nciPLC_RELOADED = "nciPLC_RELOADED";
        public const string nciRequestSettings = "nciRequestSettings";

        /// <summary>
        /// Признак потери связи соединения с сервером.
        /// </summary>
        public const string ServerDisconnected = "nviServerDisconnected";
    }
}
