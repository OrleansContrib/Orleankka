using System;
using System.Collections.Generic;

namespace Orleankka.Core
{
    using Utility;

    class ActorInvocationPipeline
    {
        readonly Dictionary<string, IActorInvoker> invokers = 
             new Dictionary<string, IActorInvoker>();

        IActorInvoker DefaultInvoker { get; set; } = DefaultActorInvoker.Instance;

        public void Register(IActorInvoker invoker)
        {
            Requires.NotNull(invoker, nameof(invoker));
            DefaultInvoker = invoker;
        }

        public void Register(string name, IActorInvoker invoker)
        {
            Requires.NotNullOrWhitespace(name, nameof(name));
            Requires.NotNull(invoker, nameof(invoker));

            invokers[name] = invoker;
        }

        public IActorInvoker GetInvoker(Type actor, string name = null)
        {
            if (name == null)
                return DefaultInvoker;

            var invoker = invokers.Find(name);
            if (invoker == null)
                throw new InvalidOperationException($"Invoker '{name}' specified for '{actor}' is not registered");

            return invoker;
        }
    }
}