using System;

namespace Orleankka
{
    class ActorGrainImplementation
    {
        public Type Interface { get; }
        public IActorMiddleware Middleware { get; }

        public ActorGrainImplementation(Type type, IActorMiddleware middleware)
        {
            Middleware = middleware;
            Interface = ActorGrain.InterfaceOf(type);
        }
    }
}