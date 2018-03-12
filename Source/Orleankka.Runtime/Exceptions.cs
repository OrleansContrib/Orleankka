using System;
using System.Runtime.Serialization;

namespace Orleankka
{
    [Serializable]
    public class UnhandledMessageException : Exception
    {
        public UnhandledMessageException(ActorGrain actor, object message, string details = "")
            : base($"An actor '{actor.GetType()}::{actor.Id}' cannot handle '{message.GetType()}'")
        {}

        UnhandledMessageException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {}
    }
}