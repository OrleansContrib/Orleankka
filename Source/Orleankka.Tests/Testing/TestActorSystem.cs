using System;

namespace Orleankka.Testing
{
    public static class TestActorSystem
    {
        public static IActorSystem Instance;

        public static ActorRef FreshActorOf<TActor>(this IActorSystem system) where TActor : Actor
        {
            return system.ActorOf<TActor>(Guid.NewGuid().ToString());
        }
    }
}
