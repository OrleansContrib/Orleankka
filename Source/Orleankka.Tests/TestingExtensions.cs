using System;
using System.Linq;

namespace Orleankka
{
    public static class TestingExtensions
    {
        public static IActorRef FreshActorOf<TActor>(this IActorSystem system)
        {
            return system.ActorOf<TActor>(Guid.NewGuid().ToString());
        }
    }
}
