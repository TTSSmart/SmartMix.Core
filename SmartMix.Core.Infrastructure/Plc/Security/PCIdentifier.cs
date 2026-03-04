namespace SmartMix.Core.Infrastructure.Plc.Security
{
    internal static class PCIdentifier
    {
        /// <summary>
        /// Генерирует номер ПК для контроллера PLC на основе идентификатора системы.
        /// Возвращает результат выполнения операции.
        /// </summary>
        /// <returns>Уникальный идентификатор ПК.</returns>
        internal static uint GetPCid()
        {
            byte[] key = new byte[2] { 0, 0, };

            string devID = "Serialog";//BSU.Utils.Security.FingerPrint.Value();
            for (int i = 0; i < devID.Length - devID.Length % 2; i += key.Length)
                for (int j = i; j < key.Length + i; j++)
                {
                    key[j - i] = (byte)(key[j - i] ^ devID[j]);
                }
            return (uint)((key[1] << 8) + key[0]);
        }
    }
}
