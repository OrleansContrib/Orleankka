using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Orleankka
{
    using Core;
    using Core.Streams;
    using Utility;

    public interface IActorSystemConfigurator
    {}

    public abstract class ActorSystemConfigurator :  MarshalByRefObject, IActorSystemConfigurator, IDisposable
    {
        readonly HashSet<EndpointConfiguration> endpoints = new HashSet<EndpointConfiguration>();
        readonly HashSet<Assembly> assemblies = new HashSet<Assembly>();

        protected void Register(params Assembly[] assemblies)
        {
            Requires.NotNull(assemblies, nameof(assemblies));

            if (assemblies.Length == 0)
                throw new ArgumentException("Assemblies length should be greater than 0", nameof(assemblies));

            foreach (var assembly in assemblies)
            {
                if (this.assemblies.Contains(assembly))
                    throw new ArgumentException($"Assembly {assembly.FullName} has been already registered");

                this.assemblies.Add(assembly);
            }
        }

        protected void Register(params EndpointConfiguration[] configs)
        {
            Requires.NotNull(configs, nameof(configs));

            if (configs.Length == 0)
                throw new ArgumentException("Configs length should be greater than 0", nameof(configs));

            foreach (var config in configs)
            {
                if (endpoints.Contains(config))
                    throw new ArgumentException($"Actor configuration with type '{config}' has been already registered");

                endpoints.Add(config);
            }
        }

        protected void Configure()
        {
            Register(ActorBinding.Bind(assemblies.ToArray()));
            ActorType.Register(endpoints.ToArray());

            var actors = endpoints.OfType<ActorConfiguration>();
            StreamSubscriptionMatcher.Register(actors.SelectMany(x => x.Subscriptions));
        }

        public void Dispose()
        {
            ActorType.Reset();
            StreamSubscriptionMatcher.Reset();
            ActorBinding.Reset();
        }

        public IEnumerable<EndpointConfiguration> Endpoints => endpoints;

        public override object InitializeLifetimeService() => null;
    }

    public abstract class ActorSystemConfiguratorExtension : MarshalByRefObject, IDisposable
    {
        protected internal abstract void Configure(IActorSystemConfigurator configurator);

        public virtual void Dispose()
        {}
    }
}