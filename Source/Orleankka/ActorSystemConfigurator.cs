using System;
using System.Collections.Generic;
using System.Linq;

using Orleankka.Core.Streams;

namespace Orleankka
{
    using Core;
    using Utility;

    public interface IActorSystemConfigurator
    {
        T[] Hooks<T>(); 
        void Hook<T>() where T : ActorSystemConfiguratorHook;
        void Register(ActorConfiguration[] configs);
    }

    public abstract class ActorSystemConfigurator :  MarshalByRefObject, IActorSystemConfigurator, IDisposable
    {
        readonly HashSet<ActorConfiguration> configs = new HashSet<ActorConfiguration>();
        readonly List<ActorSystemConfiguratorHook> hooks = new List<ActorSystemConfiguratorHook>();

        T[] IActorSystemConfigurator.Hooks<T>() => hooks.OfType<T>().ToArray();
        void IActorSystemConfigurator.Hook<T>() => hooks.Add(Activator.CreateInstance<T>());

        void IActorSystemConfigurator.Register(ActorConfiguration[] configs)
        {
            Requires.NotNull(configs, nameof(configs));

            if (configs.Length == 0)
                throw new ArgumentException("Configs length should be greater than 0", nameof(configs));

            foreach (var config in configs)
            {
                if (this.configs.Contains(config))
                    throw new ArgumentException($"Actor configuration wit code '{config}' has been already registered");

                this.configs.Add(config);
            }
        }

        protected void Configure()
        {
            hooks.ForEach(x => x.Configure(this));
            ActorType.Register(configs.ToArray());
            StreamSubscriptionMatcher.Register(configs.SelectMany(x => x.Subscriptions));
        }

        public void Dispose()
        {
            ActorType.Reset();
            StreamSubscriptionMatcher.Reset();
            hooks.ForEach(x => x.Dispose());
        }

        public override object InitializeLifetimeService() => null;
    }

    public abstract class ActorSystemConfiguratorHook : MarshalByRefObject, IDisposable
    {
        protected internal abstract void Configure(IActorSystemConfigurator configurator);

        public virtual void Dispose()
        {}
    }
}