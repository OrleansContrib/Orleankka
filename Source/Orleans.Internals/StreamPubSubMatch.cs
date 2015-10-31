using System;
using System.Threading.Tasks;

namespace Orleans.Internals
{
    /// <summary>
    /// FOR INTERNAL USE ONLY!
    /// </summary>
    public struct StreamPubSubMatch
    {
        public readonly Func<object, Task> Handler;

        public StreamPubSubMatch(Func<object, Task> handler)
        {
            Handler = handler;
        }
    }
}