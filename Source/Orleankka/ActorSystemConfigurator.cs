using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Orleankka
{
    using Core;
    using Utility;

    public interface IActorSystemConfigurator : IDisposable
    {
        void Configure(ActorSystemConfiguration configuration);
    }

    public class ActorSystemConfigurator : IActorSystemConfigurator
    {
        void IActorSystemConfigurator.Configure(ActorSystemConfiguration configuration)
        {
            Requires.NotNull(configuration, "configuration");

            if (configuration.Assemblies == null || configuration.Assemblies.Length == 0)
                throw new InvalidOperationException("No actor assemblies were specified to configure");

            if (configuration.Serializer != null && configuration.Serializer.Item1 == null)
                throw new InvalidOperationException("Serializer type cannot be a <null> reference");

            if (configuration.Activator != null && configuration.Activator.Item1 == null)
                throw new InvalidOperationException("Activator type cannot be a <null> reference");

            DoConfigure(configuration);
        }

        static void DoConfigure(ActorSystemConfiguration configuration)
        {            
            ActorAssembly.Register(configuration.Assemblies);

            if (configuration.Serializer != null)
            {
                var serializer = (IMessageSerializer) Activator.CreateInstance(configuration.Serializer.Item1);
                serializer.Init(configuration.Serializer.Item2 ?? new Dictionary<string, string>());
                MessageEnvelope.Serializer = serializer;
            }

            if (configuration.Activator != null)
            {
                var activator = (IActorActivator) Activator.CreateInstance(configuration.Activator.Item1);
                activator.Init(configuration.Activator.Item2 ?? new Dictionary<string, string>());
                ActorEndpoint.Activator = activator;
            }
        }

        void IDisposable.Dispose()
        {
            MessageEnvelope.Reset();
            ActorEndpoint.Reset();
            ActorAssembly.Reset();
        }
    }

    public class ActorSystemConfiguration
    {
        public Assembly[] Assemblies;
        public Tuple<Type, Dictionary<string, string>> Serializer;
        public Tuple<Type, Dictionary<string, string>> Activator;
    }
}