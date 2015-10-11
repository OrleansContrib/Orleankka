using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using Orleans.Runtime;

namespace Orleans.Internals
{
    /// <summary>
    /// FOR INTERNAL USE ONLY!
    /// </summary>
    public class StreamPubSubMatch
    {
        public readonly GrainReference Reference;
        public readonly Func<object, Task> Handler;

        public StreamPubSubMatch(GrainReference reference, Func<object, Task> handler)
        {
            Reference = reference;
            Handler = handler;
        }
    }
}