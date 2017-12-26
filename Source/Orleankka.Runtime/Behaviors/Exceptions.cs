using System;
using System.Runtime.Serialization;

namespace Orleankka.Behaviors
{
    [Serializable]
    public class UnhandledMessageException : Exception
    {
        public UnhandledMessageException(ActorGrain actor, object message)
            : base($"An actor '{actor.GetType()}' cannot handle '{message.GetType()}' in its current behavior '{actor.Behavior.Current}'")
        {}

        UnhandledMessageException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {}
    }

    [Serializable]
    public class UnhandledReminderException : Exception
    {
        public UnhandledReminderException(ActorGrain actor, string id)
            : base($"An actor '{actor.GetType()}' cannot handle reminder '{id}' in its current behavior '{actor.Behavior.Current}'")
        {}

        UnhandledReminderException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {}
    }
}