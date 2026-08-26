using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartMix.Core.Common
{
    public class AsyncQueueWorker<TInParam, TResult> : IDisposable where TInParam : class
    {
        private Task _task;
        private CancellationTokenSource _cts;
        private readonly SynchronizationContext _context;

        /// <summary>
        /// Представляет время задержки старта операции, в миллисекундах.
        /// </summary>
        private readonly uint _delay;

        private int _counterOfCalls;

        #region Events

        /// <summary>
        /// Происходит, когда операция успешно выполнена
        /// </summary>
        public event EventHandler<OperationEventArgs<TResult>> OperationCompleted;

        private void OnOperationCompleted(object e)
        {
            OperationCompleted?.Invoke(this, (OperationEventArgs<TResult>)e);
        }

        /// <summary>
        /// Происходит, когда операция была не выполнена из-за того, что между двумя вызовами выполнения операции прошло меньше времени, чем указано в задержке (_delay)
        /// </summary>
        public event EventHandler OperationWasAborted;

        private void OnOperationWasAborted()
        {
            OperationWasAborted?.Invoke(this, EventArgs.Empty);
        }

        #endregion Events

        /// <summary>
        /// Инициализирует новый экземпляр класса с указанным временем задержки старта операции.
        /// </summary>
        /// <param name="delay">Время задержки старта операции.</param>
        public AsyncQueueWorker(uint delay)
        {
            _context = SynchronizationContext.Current;
            _delay = delay;
        }

        /// <summary>
        /// Запускает операцию <paramref name="function"/> с отложенным временем старта <paramref name="delay"/>.
        /// </summary>
        /// <param name="function"></param>
        /// <param name="delay">Время отложенного старта, в миллисекундах.</param>
        public void StartOperation(Func<TInParam, CancellationToken, TResult> function, uint delay)
        {
            Start(function, data: null, delay);
        }

        /// <summary>
        /// Запускает операцию <paramref name="function"/> без задержки.
        /// </summary>
        /// <param name="function"></param>
        public void StartOperationNow(Func<TInParam, CancellationToken, TResult> function)
        {
            Start(function, data: null, delay: null);
        }

        /// <summary>
        /// Запускает операцию <paramref name="function"/> с отложенным временем старта по умолчанию.
        /// </summary>
        /// <param name="function"></param>
        public void StartOperation(Func<TInParam, CancellationToken, TResult> function)
        {
            Start(function, data: null, _delay);
        }

        /// <summary>
        /// Запускает операцию <paramref name="function"/> с параметрами <paramref name="data"/> без задержки.
        /// </summary>
        /// <param name="function"></param>
        /// <param name="data"></param>
        public void StartOperationNow(Func<TInParam, CancellationToken, TResult> function, TInParam data)
        {
            Start(function, data, delay: null);
        }

        /// <summary>
        /// Запускает операцию <paramref name="function"/> с параметрами <paramref name="data"/> и отложенным временем старта по умолчанию.
        /// </summary>
        /// <param name="function"></param>
        /// <param name="data"></param>
        public void StartOperation(Func<TInParam, CancellationToken, TResult> function, TInParam data)
        {
            Start(function, data, _delay);
        }

        /// <summary>
        /// Запускает операцию <paramref name="function"/> с параметрами <paramref name="data"/> и отложенным временем старта <paramref name="delay"/>
        /// </summary>
        /// <param name="function"></param>
        /// <param name="data"></param>
        /// <param name="delay">Время задержки старта операции, в миллисекундах</param>
        public void StartOperation(Func<TInParam, CancellationToken, TResult> function, TInParam data, uint delay)
        {
            Start(function, data, delay);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="function"></param>
        /// <param name="data"></param>
        /// <param name="delay">Время задержки старта операции, в миллисекундах</param>
        private async void Start(Func<TInParam, CancellationToken, TResult> function, TInParam data, uint? delay)
        {
            if (delay.HasValue && delay > 0)
            {
                // отложенный старт операции
                Interlocked.Increment(ref _counterOfCalls);

                await Task.Delay(TimeSpan.FromMilliseconds(delay.Value));

                int currentValue = Interlocked.Decrement(ref _counterOfCalls);
                if (currentValue == 0)
                    Execute(function, data);
            }
            else
            {
                // немедленный запуск операции
                Execute(function, data);
            }
        }

        private void Execute(Func<TInParam, CancellationToken, TResult> function, TInParam data)
        {
            if (_task != null)
            {
                // отменяем предыдущую задачу
                try
                {
                    _cts?.Cancel();
                    _task.Dispose();
                }
                catch (Exception)
                {
                    // ignore
                }
                finally
                {
                    _cts?.Dispose();
                }
            }

            _cts = new CancellationTokenSource(); // инициализация
            _task = new Task(OnExecute, new Tuple<Func<TInParam, CancellationToken, TResult>, TInParam, CancellationToken>(function, data, _cts.Token), _cts.Token, TaskCreationOptions.PreferFairness);
            _task.Start(TaskScheduler.Default);
        }

        private void OnExecute(object state)
        {
            Exception exception = null;
            TResult result = default(TResult);
            CancellationToken cts = CancellationToken.None;
            try
            {
                var param = (Tuple<Func<TInParam, CancellationToken, TResult>, TInParam, CancellationToken>)state;
                if (param.Item1 != null) // метод был определен
                {
                    cts = param.Item3;
                    if (cts.IsCancellationRequested)
                    {
                        OnOperationWasAborted();
                    }
                    else
                    {
                        result = param.Item1.Invoke(param.Item2, cts); // получили результат

                        if (cts.IsCancellationRequested)
                            OnOperationWasAborted();
                    }
                }
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            finally
            {
                _context.Post(OnOperationCompleted, new OperationEventArgs<TResult>(result, cts.IsCancellationRequested, exception));
            }
        }

        #region IDisposable Members

        private bool _disposed;

        private void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                // Free any other managed objects here.
                try
                {
                    _cts?.Cancel();
                    _cts?.Dispose();
                    _task?.Dispose();
                }
                catch (Exception)
                {
                }
            }

            // Free any unmanaged objects here.
            _disposed = true;
        }

        /// <summary>
        /// Выполняет определяемые приложением задачи, связанные с высвобождением или сбросом неуправляемых ресурсов.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Members

        /// <inheritdoc/>
        ~AsyncQueueWorker()
        {
            Dispose(false);
        }
    }
}
