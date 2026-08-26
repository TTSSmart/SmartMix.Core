using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartMix.Core.Common
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class OperationEventArgs<T> : EventArgs
    {
        public OperationEventArgs(T result, bool isCancellationRequested, Exception error)
        {
            Result = result;
            IsCancellationRequested = isCancellationRequested;
            Error = error;
        }

        public bool IsCancellationRequested { get; protected set; }

        public T Result { get; protected set; }

        public Exception Error { get; protected set; }
    }
}
