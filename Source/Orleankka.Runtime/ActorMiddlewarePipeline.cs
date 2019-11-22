using System;
using System.Collections.Generic;
using System.Linq;

using Orleankka.Utility;

namespace Orleankka
{
    class ActorMiddlewarePipeline
    {
        readonly List<(Type type, IActorMiddleware middleware)> middlewares = 
             new List<(Type, IActorMiddleware)>();

        IActorMiddleware DefaultMiddleware { get; set; } = DefaultActorMiddleware.Instance;

        public void Register(IActorMiddleware middleware)
        {
            Requires.NotNull(middleware, nameof(middleware));
            DefaultMiddleware = middleware;
        }

        public void Register(Type actor, IActorMiddleware middleware)
        {
            Requires.NotNull(actor, nameof(actor));
            Requires.NotNull(middleware, nameof(middleware));

            if (middlewares.Any(x => x.type == actor))
                throw new InvalidOperationException($"Middleware for {actor} is already registered");

            middlewares.Add((actor, middleware));
        }

        public IActorMiddleware Middleware(Type actor)
        {
            var registered = middlewares.FirstOrDefault(x => x.type.IsAssignableFrom(actor));
            return registered.middleware ?? DefaultMiddleware;
        }
    }
}