using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartMix.Core.Infrastructure.Persistence.Database
{
    public class RepositoryFactory : IRepositoryFactory
    {
        private static Lazy<IRepositoryFactory> _lazy = new Lazy<IRepositoryFactory>(() => new RepositoryFactory());

        private readonly Dictionary<string, Func<object>> _repositories = new Dictionary<string, Func<object>>();

        /// <summary>
        /// Представляет адаптер к БД.
        /// </summary>
        private ISmartMixDatabase _ISmartMixDatabase;

        public static IRepositoryFactory Instance => _lazy.Value;

        /// <summary>
        /// Инициализирует подключение к БД
        /// </summary>
        /// <typeparam name="T">Тип БД</typeparam>
        /// <param name="connectionString">Строка подключения</param>
        public void InitializeDb<T>(string connectionString) where T : DbConnection, new()
        {
            _ISmartMixDatabase = new SmartMixSqlDatabase<T>(connectionString);
        }
        /// <summary>
        /// Добавить класс репозитория, который должен иметь конструктор с одним аргументом типа <see cref="ISmartMixDatabase"/>
        /// </summary>
        /// <typeparam name="TInput">Интерфейс репозитория, используется для ключа</typeparam>
        /// <typeparam name="TOutput">Конкретный класс</typeparam>
        public void Add<TInput, TOutput>()
        {
            if (_repositories.TryGetValue(typeof(TInput).Name, out _))
                throw new Exception($"Тип {typeof(TInput).Name} уже добавлен");

            if (_ISmartMixDatabase == null)
                throw new Exception($"Перед добавление нужно вызвать метода {nameof(InitializeDb)}");

            _repositories.Add(typeof(TInput).Name, () => Activator.CreateInstance(typeof(TOutput), _ISmartMixDatabase));
        }

        /// <summary>
        /// Получить репозиторий
        /// </summary>
        /// <typeparam name="TOutput">Интерфейс репозитория</typeparam>
        /// <returns></returns>
        public TOutput Get<TOutput>()
        {
            if (_repositories.TryGetValue(typeof(TOutput).Name, out Func<object> func))
                return (TOutput)func.Invoke();

            throw new Exception($"Тип {nameof(TOutput)} не найден");
        }

    }
}
