namespace shop.commerce.api.common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// the list extensions
    /// </summary>
    public static class ListExtentions
    {
        /// <summary>
        /// get the value of the key or default value if the key not
        /// </summary>
        /// <typeparam name="TKey">the type of the key</typeparam>
        /// <typeparam name="TValue">the value of the key</typeparam>
        /// <param name="dictionary">the source</param>
        /// <param name="key">the key to look for</param>
        /// <param name="defaultValue">the default value to return if noting found, by default set to <see cref="default"/></param>
        /// <returns>the value, or default if not fount</returns>
        public static TValue Find<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default)
        {
            if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));
            return dictionary.TryGetValue(key, out var result) ? result : defaultValue;
        }

        /// <summary>
        /// get the value of the key or default value if the key not
        /// </summary>
        /// <typeparam name="TKey">the type of the key</typeparam>
        /// <typeparam name="TValue">the value of the key</typeparam>
        /// <param name="dictionary">the source</param>
        /// <param name="key">the key to look for</param>
        /// <param name="key">the default value to return if noting found, by default set to <see cref="default"/></param>
        /// <returns>the value, or default if not fount</returns>
        public static TValue Find<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
            => Find(dictionary, key, default);

        /// <summary>
        /// split the list into small set of lists of the given size
        /// </summary>
        /// <typeparam name="T">the type of the type of the list elements</typeparam>
        /// <param name="source">the list to split</param>
        /// <param name="chunkSize">the size of the small set of lists</param>
        /// <returns>return an <see cref="IEnumerable{T}"/> of the split lists</returns>
        public static IEnumerable<IEnumerable<TSource>> Chunk<TSource>(this IEnumerable<TSource> source, int chunkSize)
        {
            while (source.Any())
            {
                yield return source.Take(chunkSize);
                source = source.Skip(chunkSize);
            }
        }

        /// <summary>
        /// select the entity identifier and create a string list from it, output example : [1,2,3] or ['MA','FR']
        /// </summary>
        /// <typeparam name="T">the type of the entity</typeparam>
        /// <typeparam name="Tkey">the type of the key</typeparam>
        /// <param name="list">the list instant</param>
        /// <param name="keySelector">the key selector</param>
        /// <param name="listBounds">the type of the list Bounds defaulted to <see cref="ListBounds.Brackets"/></param>
        /// <param name="separator">the type of the separator, defaulted to ","</param>
        /// <returns>the string list of identifier</returns>
        public static string SelectIdentifier<T, Tkey>(this IEnumerable<T> list, Func<T, Tkey> keySelector, ListBounds listBounds = ListBounds.Brackets, string separator = ",")
        {
            var firstBound = listBounds == ListBounds.Brackets ? "[" : listBounds == ListBounds.parentheses ? "(" : "{";
            var secondBound = listBounds == ListBounds.Brackets ? "]" : listBounds == ListBounds.parentheses ? ")" : "}";
            if (typeof(Tkey) == typeof(string) || typeof(Tkey) == typeof(Guid))
            {
                separator = $"'{separator}'";
                return $"{firstBound}'{string.Join(separator, list.Select(keySelector))}'{secondBound}";
            }

            return $"{firstBound}{string.Join(separator, list.Select(keySelector))}{secondBound}";
        }

        /// <summary>
        /// this function will yield return the item
        /// </summary>
        /// <typeparam name="T">the type of the item</typeparam>
        /// <param name="item">the item instant</param>
        /// <returns>an enumerable</returns>
        public static IEnumerable<T> Yield<T>(this T item)
        {
            yield return item;
        }

        /// <summary>
        /// get the interactions of two sequence
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns>an instant of Intersection model</returns>
        public static Intersection<TEntity> Intersection<TEntity>(this IEnumerable<TEntity> first, IEnumerable<TEntity> second)
            => new Intersection<TEntity>
            {
                Common = second.Intersect(first),
                Removed = first.Except(second),
                Added = second.Except(first),
            };

        /// <summary>
        /// get the interactions of two sequence
        /// </summary>
        /// <typeparam name="TEntity">the type of the entity</typeparam>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <param name="comparer"></param>
        /// <returns></returns>
        public static Intersection<TEntity> Intersection<TEntity>(this IEnumerable<TEntity> first, IEnumerable<TEntity> second, IEqualityComparer<TEntity> comparer)
            => new Intersection<TEntity>
            {
                Common = second.Intersect(first, comparer),
                Removed = first.Except(second, comparer),
                Added = second.Except(first, comparer),
            };

        /// <summary>
        /// check if the given array has any value
        /// </summary>
        /// <typeparam name="TType">the type of the data</typeparam>
        /// <param name="data">the list of data</param>
        /// <returns>true if has any data false if not</returns>
        public static bool HasValue<TType>(this IEnumerable<TType> data)
            => !(data is null) && data.Any();

        public static IEnumerable<TSource> FromHierarchy<TSource>(this TSource source, Func<TSource, TSource> nextItem, Func<TSource, bool> canContinue)
        {
            for (var current = source; canContinue(current); current = nextItem(current))
            {
                yield return current;
            }
        }

        public static IEnumerable<TSource> FromHierarchy<TSource>(this TSource source, Func<TSource, TSource> nextItem) 
            where TSource : class => FromHierarchy(source, nextItem, s => s != null);
    }
}
