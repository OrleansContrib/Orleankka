using System;

namespace Orleankka.Testing
{
    using Embedded;

    public static class TestActorSystem
    {
        public static EmbeddedActorSystem Instance;

        public static ActorRef FreshActorOf<TActor>(this IActorSystem system) where TActor : Actor
        {
            return system.ActorOf<TActor>(Guid.NewGuid().ToString());
        }
    }
}
