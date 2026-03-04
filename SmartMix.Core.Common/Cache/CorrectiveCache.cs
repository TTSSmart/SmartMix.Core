using System.Runtime.Caching;

namespace SmartMix.Core.Common.Cache
{
    /// <summary>
    /// Представляет кеш оперативных корректировок по заявкам из очереди выполнения.
    /// </summary>
    public class CorrectiveCache
    {
        /// <summary>
        /// Представляет кеш текущих оперативных корректировок по заявкам из очереди выполнения, где ключом является составной идентификатор в формате "ID заявки_ID рецепта".
        /// </summary>
        private readonly MemoryCache _cache;

        /// <summary>
        /// Представляет политики записи кеша.
        /// </summary>
        private readonly CacheItemPolicy _policy;

        private object _locker = new object();

        /// <summary>
        /// Инициализирует новый экземпляр класса.
        /// </summary>
        /// <param name="hours">Время жизни кеша, в часах.</param>
        public CorrectiveCache(int hours = 6)
        {
            _cache = new MemoryCache("corrective");
            _policy = new CacheItemPolicy() { SlidingExpiration = TimeSpan.FromHours(hours) };
        }

        /// <summary>
        /// Определяет, содержится ли в кеше запись по указанному ключу <paramref name="key"/>.
        /// </summary>
        /// <param name="key">Составной идентификатор "ID заявки_ID рецепта".</param>
        /// <returns>Значение <see langword="true"/>, если кеш содержит запись с указанным ключом, иначе - значение <see langword="false"/>.</returns>
        public bool Contains(string key)
        {
            lock (_locker)
            {
                return _cache.Contains(key);
            }
        }

        /// <summary>
        /// Получает значение по указанному ключу.
        /// </summary>
        /// <param name="key">Составной идентификатор "ID заявки_ID рецепта".</param>
        /// <returns>Словарь оперативных корректировок, если ключ был найден, иначе - значение <see langword="null"/>.</returns>
        public Dictionary<int, float> TryGet(string key)
        {
            lock (_locker)
            {
                if (_cache.Contains(key))
                    return _cache.Get(key) as Dictionary<int, float>;

                return null;
            }
        }

        /// <summary>
        /// Добавляет запись в кеш, если ключа <paramref name="key"/> не существует, или обновляет текущую запись кеша.
        /// </summary>
        /// <param name="key">Составной идентификатор "ID заявки_ID рецепта".</param>
        /// <param name="value">Полный словарь оперативных корректировок, где ключом является идентификатор компонента, а значением - <see cref="BSU.API.Data.Recipes.RecipeStructure.Correct"/>.</param>
        public void Set(string key, Dictionary<int, float> value)
        {
            lock (_locker)
            {
                //Add or Update
                _cache.Set(key, value, _policy);
            }
        }

        /// <summary>
        /// Удаляет значение по указанному ключу.
        /// </summary>
        /// <param name="key">Составной идентификатор "ID заявки_ID рецепта".</param>
        public void TryRemove(string key)
        {
            lock (_locker)
            {
                if (_cache.Contains(key))
                    _cache.Remove(key);
            }
        }

        /// <summary>
        /// Возвращает текущий словарь оперативных корректировок.
        /// </summary>
        /// <returns>Словарь оперативных корректировок по заявкам, где ключом является "ID заявки_ID рецепта".</returns>
        public Dictionary<string, Dictionary<int, float>> Values()
        {
            lock (_locker)
            {
                List<KeyValuePair<string, object>> values = new List<KeyValuePair<string, object>>();
                values.AddRange(_cache);

                return values.ToDictionary(x => x.Key, x => (Dictionary<int, float>)x.Value);
            }
        }

        /// <summary>
        /// Возвращает количество записей в кеше.
        /// </summary>
        public long Length
        {
            get
            {
                lock (_locker)
                {
                    return _cache.GetCount();
                }
            }
        }
    }
}
