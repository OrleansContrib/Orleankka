using System;

namespace Orleankka.Testing
{
    using CSharp;

    public static class TestActorSystem
    {
        public static IActorSystem Instance;

        public static ActorRef FreshActorOf<TActor>(this IActorSystem system) where TActor : Actor
        {
            return system.ActorOf<TActor>(Guid.NewGuid().ToString());
        }
    }
}
