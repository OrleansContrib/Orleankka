using System;
using System.Diagnostics;

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
    }
}