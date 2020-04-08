using System;
using System.Collections;
using System.Collections.Generic;

namespace BannerLord.Common.Extensions
{
    public static class EnumerableExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var obj in enumerable)
            {
                action(obj);
            }
        }

        public static string StringJoin(this IEnumerable<object> enumerable, string separator = ", ")
        {
            return string.Join(separator, enumerable);
        }


        public static IEnumerable<T> NullToEmpty<T>(this IEnumerable<T> enumerable)
        {
            return enumerable ?? new T[] { };
        }
        
        public static int IndexOf<TSource>(this IEnumerable<TSource> source, TSource item,
            IEqualityComparer<TSource> itemComparer = null)
        {
            switch (source)
            {
                case null:
                    throw new ArgumentNullException(nameof(source));
                case IList<TSource> listOfT:
                    return listOfT.IndexOf(item);
                case IList list:
                    return list.IndexOf(item);
            }

            if (itemComparer == null)
            {
                itemComparer = EqualityComparer<TSource>.Default;
            }

            var i = 0;
            foreach (var possibleItem in source)
            {
                if (itemComparer.Equals(item, possibleItem))
                {
                    return i;
                }
                i++;
            }
            return -1;
        }
    }
}