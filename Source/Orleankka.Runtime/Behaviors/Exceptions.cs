using System;
using System.Runtime.Serialization;

namespace Orleankka.Behaviors
{
    [Serializable]
    public class UnhandledMessageException : Exception
    {
        public UnhandledMessageException(Type actor, string state, object message)
            : base($"An actor '{actor}' cannot handle '{message.GetType()}' in its current state '{state}'")
        {}

        UnhandledMessageException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {}
    }

    [Serializable]
    public class UnhandledReminderException : Exception
    {
        public UnhandledReminderException(Type actor, string state, string reminder)
            : base($"An actor '{actor}' cannot handle reminder '{reminder}' in its current state '{state}'")
        {}

        UnhandledReminderException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {}
    }
}