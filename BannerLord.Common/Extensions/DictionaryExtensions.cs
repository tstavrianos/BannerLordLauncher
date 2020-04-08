using System;
using System.Collections.Generic;

namespace BannerLord.Common.Extensions
{
    public static class DictionaryExtensions
    {
        public static TV ComputeIfAbsent<TK, TV>(this IDictionary<TK, TV> dictionary, TK key, Func<TK, TV> func)
        {
            if (!dictionary.ContainsKey(key))
            {
                dictionary[key] = func(key);
            }

            return dictionary[key];
        }
        
        public static void PutAll<TK, TV>(this IDictionary<TK, TV> dictionary,
            IEnumerable<KeyValuePair<TK, TV>> toPut)
        {
            foreach (var (key, value) in toPut)
            {
                dictionary[key] = value;
            }
        }
        
        internal static TValue GetOrCreate<TKey, TValue>(this Dictionary<TKey, TValue> map, TKey key, Func<TKey, TValue> ctor)
        {
            if (!map.ContainsKey(key))
            {
                map[key] = ctor(key);
            }
            return map[key];
        }

    }
}