using System;
using System.Runtime.Serialization;

namespace Orleankka.Behaviors
{
    [Serializable]
    public class UnhandledMessageException : Exception
    {
        public UnhandledMessageException(Type actor, string behavior, object message)
            : base($"An actor '{actor}' cannot handle '{message.GetType()}' in its current behavior '{behavior}'")
        {}

        UnhandledMessageException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {}
    }

    [Serializable]
    public class UnhandledReminderException : Exception
    {
        public UnhandledReminderException(Type actor, string behavior, string reminder)
            : base($"An actor '{actor}' cannot handle reminder '{reminder}' in its current behavior '{behavior}'")
        {}

        UnhandledReminderException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {}
    }
}