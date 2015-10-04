using System;
using System.Collections.Concurrent;

namespace Orleankka.Core
{
    // NOTE: WeakReferences and timer-based cleanup could be used here.
    //       Might be useful for cases when there is a huge number of short-lived streams.

    class StreamCache
    {
        readonly ConcurrentDictionary<string, object> streams =
             new ConcurrentDictionary<string, object>();

        internal Stream<T> GetOrAdd<T>(string id, Func<Stream<T>> factory)
        {
            return streams.GetOrAdd(id, _ => factory()) as Stream<T>;
        }
    }
}