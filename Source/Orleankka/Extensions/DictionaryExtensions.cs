using System;

namespace System.Collections.Generic
{
    static class DictionaryExtensions
    {
        public static TResult Find<TKey, TResult>(this IDictionary<TKey, TResult> dictionary, TKey key) where TResult : class
        {
            TResult result;
            return !dictionary.TryGetValue(key, out result) ? null : result;
        }
    }
}
