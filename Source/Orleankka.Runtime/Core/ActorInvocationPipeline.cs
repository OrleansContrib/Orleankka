using System;
using System.Collections.Generic;
using System.Linq;

namespace Orleankka.Core
{
    using Utility;

    public class ActorInvocationPipeline
    {
        readonly List<(Type type, IActorInvoker invoker)> invokers = 
             new List<(Type, IActorInvoker)>();

        IActorInvoker DefaultInvoker { get; set; } = DefaultActorInvoker.Instance;

        public void Register(IActorInvoker invoker)
        {
            Requires.NotNull(invoker, nameof(invoker));
            DefaultInvoker = invoker;
        }

        public void Register(Type actor, IActorInvoker invoker)
        {
            Requires.NotNull(actor, nameof(actor));
            Requires.NotNull(invoker, nameof(invoker));

            if (invokers.Any(x => x.type == actor))
                throw new InvalidOperationException($"Invoker for {actor} is already registered");

            invokers.Add((actor, invoker));
        }

        public IActorInvoker GetInvoker(Type actor)
        {
            var registered = invokers.FirstOrDefault(x => x.type.IsAssignableFrom(actor));
            return registered.invoker ?? DefaultInvoker;
        }
    }
}