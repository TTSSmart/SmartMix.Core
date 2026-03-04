namespace SmartMix.Core.Infrastructure.Plc.Models
{
    internal class Waiter<T> where T : IEquatable<T>
    {
        public delegate T Func();

        private readonly Func _function;
        private readonly int _timeDelay;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="function"></param>
        /// <param name="timeDelay">Количество тактов ожидания по <see cref="func"/> в миллисекундах.</param>
        public Waiter(Func function, int timeDelay)
        {
            _function = function;
            _timeDelay = timeDelay;
        }

        /// <summary>
        /// Повтор метода и ожидание результирующего значения <paramref name="expectedValue"/>.
        /// </summary>
        /// <param name="expectedValue">Ожидаемое значение</param>
        /// <param name="countDelay">Количество повторов ожидания результата по timeDelay миллисекунд.</param>
        /// <returns></returns>
        public async Task<bool> Wait(T expectedValue, int countDelay)
        {
            return await Task.Run(async () =>
            {
                int k = 0;

                while (k < countDelay)
                {
                    T curVal = _function();

                    if (curVal.Equals(expectedValue))
                        return true;

                    k++;
                    await Task.Delay(_timeDelay);
                }
                return false;
            });
        }
    }
}
