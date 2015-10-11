using System;
using System.Diagnostics;
using System.Collections.Generic;

using Orleans.Providers.Streams.Common;

namespace Orleans.Internals
{
    public static class UtilityExtensions
    {
        public static bool IsPersistentStreamProvider(this Type type)
        {
            Debug.Assert(type.BaseType != null);
            return type.BaseType.IsConstructedGenericType &&
                   type.BaseType.GetGenericTypeDefinition() == typeof(PersistentStreamProvider<>);
        }

        public static TResult Find<TKey, TResult>(this IDictionary<TKey, TResult> dictionary, TKey key) where TResult : class
        {
            TResult result;
            return !dictionary.TryGetValue(key, out result) ? null : result;
        }
    }
}