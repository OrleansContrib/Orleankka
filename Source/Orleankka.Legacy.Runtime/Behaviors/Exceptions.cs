using System;
using System.Runtime.Serialization;

using Orleans;

namespace Orleankka.Legacy.Behaviors
{
    [GenerateSerializer, Serializable]
    public class UnhandledReminderException : Exception
    {
        public UnhandledReminderException(Actor actor, string id)
            : base($"An actor '{actor.GetType()}' cannot handle reminder '{id}' in its current behavior '{actor.Behavior.Current}'")
        {}

        UnhandledReminderException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {}
    }
}