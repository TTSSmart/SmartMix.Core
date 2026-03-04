namespace SmartMix.Core.Infrastructure.Plc.Models
{
    public class WriteVariableResult
    {
        public WriteVariableResult(bool result, string error = "")
        {
            Result = result;
            Error = error;
        }

        /// <summary>Результат</summary>  
        public bool Result { get; protected set; }

        /// <summary>Текст ошибки, возвращается, если возникло исключение</summary>
        public string Error { get; protected set; }
    }
}
