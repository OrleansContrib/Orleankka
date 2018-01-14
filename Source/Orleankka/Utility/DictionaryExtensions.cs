using System.Collections.Generic;

namespace Orleankka.Utility
{
    static class DictionaryExtensions
    {
        public static TResult Find<TKey, TResult>(this IDictionary<TKey, TResult> dictionary, TKey key) where TResult : class => 
            !dictionary.TryGetValue(key, out var result) ? null : result;
    }
}
