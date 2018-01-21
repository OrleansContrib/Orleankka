using System;
using System.Runtime.Serialization;

namespace Orleankka.Behaviors
{
    [Serializable]
    public class UnhandledMessageException : Exception
    {
        public UnhandledMessageException(ActorGrain actor, object message)
            : base($"An actor '{actor.GetType()}::{actor.Id}' cannot handle '{message.GetType()}' in its current behavior '{actor.Behavior.Current}'")
        {}

        UnhandledMessageException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {}
    }
}