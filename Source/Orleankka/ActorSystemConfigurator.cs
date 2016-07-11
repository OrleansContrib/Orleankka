using System;
using System.Collections.Generic;
using System.Linq;

namespace Orleankka
{
    using Core;
    using Core.Streams;
    using Utility;

    public interface IActorSystemConfigurator
    {
        void Register(EndpointConfiguration[] configs);
    }

    public interface IExtensibleActorSystemConfigurator
    {
        void Extend<T>(Action<T> configure) where T : ActorSystemConfiguratorExtension, new();
    }

    public abstract class ActorSystemConfigurator :  MarshalByRefObject, IActorSystemConfigurator, IExtensibleActorSystemConfigurator, IDisposable
    {
        readonly HashSet<EndpointConfiguration> configs = new HashSet<EndpointConfiguration>();
        readonly List<ActorSystemConfiguratorExtension> extensions = new List<ActorSystemConfiguratorExtension>();

        void IExtensibleActorSystemConfigurator.Extend<T>(Action<T> configure)
        {
            var extension = Add<T>();
            configure(extension);
        }

        internal T Add<T>() where T : ActorSystemConfiguratorExtension
        {
            var extension = Activator.CreateInstance<T>();
            extensions.Add(extension);
            return extension;
        }

        void IActorSystemConfigurator.Register(EndpointConfiguration[] configs)
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
            extensions.ForEach(x => x.Configure(this));
            ActorType.Register(configs.ToArray());

            var actors = configs.OfType<ActorConfiguration>();
            StreamSubscriptionMatcher.Register(actors.SelectMany(x => x.Subscriptions));
        }

        public void Dispose()
        {
            ActorType.Reset();
            StreamSubscriptionMatcher.Reset();
            extensions.ForEach(x => x.Dispose());
        }

        public override object InitializeLifetimeService() => null;
    }

    public abstract class ActorSystemConfiguratorExtension : MarshalByRefObject, IDisposable
    {
        protected internal abstract void Configure(IActorSystemConfigurator configurator);

        public virtual void Dispose()
        {}
    }
}