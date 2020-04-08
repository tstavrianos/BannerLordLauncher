using System.Collections.Generic;

namespace BannerLord.Common.Extensions
{
    public static class ListExtensions
    {
        public static void Swap<T>(this IList<T> list, int a, int b)
        {
            var tmp = list[a];
            list[a] = list[b];
            list[b] = tmp;
        }
        
        public static void Splice<T>(this List<T> input, int start, int count, params T[] objects) 
            => input.Splice(start, count, (IEnumerable<T>) objects);

        
        public static void Splice<T>(this List<T> input, int start, int count, IEnumerable<T> objects)
        {
            input.RemoveRange(start, count);
            input.InsertRange(start, objects);
        }

    }
}