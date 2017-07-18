using System;
using System.Collections.Generic;

namespace Orleankka
{
    using Utility;

    public interface IInvocationPipeline
    {
        /// <summary>
        /// Registers default actor invoker. This invoker will be used for every actor 
        /// which doesn't specify an individual invoker via <see cref="InvokerAttribute"/> attribute.
        /// </summary>
        /// <param name="invoker">The invoker.</param>
        void Register(IActorInvoker invoker);

        /// <summary>
        /// Registers named actor invoker. For this invoker to be used an actor need 
        /// to specify its name via <see cref="InvokerAttribute"/> attribute.
        /// </summary>
        /// <param name="name">The name of the invoker</param>
        /// <param name="invoker">The invoker.</param>
        void Register(string name, IActorInvoker invoker);
    }

    class InvocationPipeline : IInvocationPipeline
    {
        public static InvocationPipeline Instance { get; private set; } = new InvocationPipeline();

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

        public IActorInvoker GetInvoker(Type actor, string name)
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