namespace SmartMix.Core.Common.Extentions
{
    public static class IEnumerableExtention
    {
        public static void ForEach<T>(this IEnumerable<T> @this, Action<T> action) // todo убрать это
        {
            foreach (T item in @this)
            {
                action(item);
            }
        }

        /// <summary>
        /// Получаем только уникальные объекты
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="source"></param>
        /// <param name="keySelector"></param>
        /// <returns></returns>
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            var uniqueKeys = new HashSet<TKey>();
            foreach (var item in source)
            {
                TKey key = keySelector(item);
                if (uniqueKeys.Add(key))
                {
                    yield return item;
                }
            }
        }
    }
}
