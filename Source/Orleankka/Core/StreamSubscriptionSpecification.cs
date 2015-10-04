using System;

namespace Orleankka.Core
{
    class StreamSubscriptionSpecification
    {
        readonly string source;
        readonly string target;
        readonly Type actor;

        public StreamSubscriptionSpecification(string source, string target, Type actor)
        {
            this.source = source;
            this.target = target;
            this.actor = actor;
        }

        public bool Matches(string stream)
        {
            return source == stream;
        }

        public ActorRef Target(IActorSystem system, string stream)
        {
            return system.ActorOf(actor, target);
        }
    }
}