using System;
using System.Linq;

using Orleankka.Typed;

namespace Orleankka.Testing
{
    public static class TestActorSystem
    {
        public static IActorSystem Instance;

        public static ActorRef FreshActorOf<TActor>(this IActorSystem system) where TActor : Actor
        {
            return system.ActorOf<TActor>(Guid.NewGuid().ToString());
        }

        public static TypedActorRef<TActor> FreshTypedActorOf<TActor>(this IActorSystem system) where TActor : TypedActor
        {
            return new TypedActorRef<TActor>(system.FreshActorOf<TActor>());
        }

        public static TypedActorRef<TActor> Typed<TActor>(this ActorRef @ref) where TActor : TypedActor
        {
            return new TypedActorRef<TActor>(@ref);
        }
    }
}
