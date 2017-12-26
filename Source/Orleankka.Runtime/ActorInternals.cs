using Orleankka.Core;

namespace Orleankka
{
    namespace Internals
    {
        public static class ActorExtensions
        {
            public static IActorHost Host(this ActorGrain actor) => actor.Host;
        }
    }
}
