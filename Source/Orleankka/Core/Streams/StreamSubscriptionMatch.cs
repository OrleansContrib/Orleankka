using System;
using System.Linq;

namespace Orleankka.Core.Streams
{
    struct StreamSubscriptionMatch
    {
        public static readonly StreamSubscriptionMatch None = new StreamSubscriptionMatch();

        public readonly Type ActorType;
        public readonly string ActorId;
        public readonly Func<object, bool> Filter;

        public StreamSubscriptionMatch(Type actorType, string actorId, Func<object, bool> filter)
        {
            ActorType = actorType;
            ActorId = actorId;
            Filter = filter;
        }

        public StreamConsumer Consumer(IActorSystem system)
        {
            var actor = system.ActorOf(ActorType, ActorId);
            return new StreamConsumer(actor, Filter);
        }
    }
}