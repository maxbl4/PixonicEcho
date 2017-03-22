using System;
using System.Collections.Generic;

namespace PixonicEcho
{
    static class DictionaryExtensions
    {
        public static TValue AddSync<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue value)
        {
            lock (dict)
            {
                dict[key] = value;
                return value;
            }
        }

        public static TValue GetSync<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue defaulValue = default (TValue))
        {
            lock (dict)
            {
                TValue value;
                if (dict.TryGetValue(key, out value))
                    return value;
                return defaulValue;
            }
        }

        public static TValue GetOrAddSync<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, Func<TKey, TValue> valueFactory)
        {
            lock (dict)
            {
                TValue value;
                if (dict.TryGetValue(key, out value))
                    return value;
                return dict[key] = valueFactory(key);
            }
        }

        public static bool TryRemoveSync<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, out TValue value)
        {
            lock (dict)
            {
                var res = dict.TryGetValue(key, out value);
                dict.Remove(key);
                return res;
            }
        }

        public static bool RemoveWhereSync<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, Func<TKey, TValue, bool> condition, out TValue value)
        {
            lock (dict)
            {
                value = default (TValue);
                if (dict.TryGetValue(key, out value) && condition(key, value))
                {
                    dict.Remove(key);
                    return true;
                }
                return false;
            }
        }
    }
}