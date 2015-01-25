using System;
using System.Linq;

using Orleans.Concurrency;

namespace Orleankka.Dynamic
{
    /// <summary> 
    /// FOR INTERNAL USE ONLY! 
    /// </summary>
    [Immutable, Serializable]
    public sealed class DynamicNotification
    {
        public readonly ActorPath Source;
        public readonly byte[] Message;

        public DynamicNotification(ActorPath source, byte[] message)
        {
            Source = source;
            Message = message;
        }
    }
}
