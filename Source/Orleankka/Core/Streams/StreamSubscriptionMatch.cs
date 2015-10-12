using System;
using System.Linq;

namespace Orleankka.Core.Streams
{
    struct StreamSubscriptionMatch
    {
        public static readonly StreamSubscriptionMatch None = new StreamSubscriptionMatch();

        public readonly Type ActorType;
        public readonly string ActorId;

        public StreamSubscriptionMatch(Type actorType, string actorId)
        {
            ActorId = actorId;
            ActorType = actorType;
        }

        public StreamConsumer Consumer(IActorSystem system)
        {
            var actor = system.ActorOf(ActorType, ActorId);
            return new StreamConsumer(actor);
        }
    }
}