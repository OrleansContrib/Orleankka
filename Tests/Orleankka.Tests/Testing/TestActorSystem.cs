using System;

using Orleans;
using Orleans.Hosting;

namespace Orleankka.Testing
{
    using Client;

    public static class TestActorSystem
    {
        public static ISiloHost Host;
        public static IClusterClient Client;
        public static IClientActorSystem Instance;

        public static ActorRef FreshActorOf<TActor>(this IActorSystem system) where TActor : IActorGrain, IGrainWithStringKey =>
            system.ActorOf<TActor>(Guid.NewGuid().ToString());

        public static ActorRef<TActor> FreshTypedActorOf<TActor>(this IActorSystem system) where TActor : IActorGrain, IGrainWithStringKey =>
            system.TypedActorOf<TActor>(Guid.NewGuid().ToString());
    }
}
