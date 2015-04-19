using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Orleankka
{
    using Core;
    using Utility;

    public interface IActorSystemConfigurator
    {}

    public abstract class ActorSystemConfigurator : MarshalByRefObject, IDisposable
    {
        readonly HashSet<Assembly> assemblies = new HashSet<Assembly>();
         
        Tuple<Type, Dictionary<string, string>> serializer;
        Tuple<Type, Dictionary<string, string>> activator;

        protected void RegisterSerializer<T>(Dictionary<string, string> properties = null) where T : IMessageSerializer
        {
            if (serializer != null)
                throw new InvalidOperationException("Serializer has been already registered");

            serializer = Tuple.Create(typeof(T), properties);
        }

        protected void RegisterActivator<T>(Dictionary<string, string> properties = null) where T : IActorActivator
        {
            if (activator != null)
                throw new InvalidOperationException("Activator has been already registered");

            activator = Tuple.Create(typeof(T), properties);
        }

        protected void RegisterAssemblies(params Assembly[] assemblies)
        {
            Requires.NotNull(assemblies, "assemblies");

            if (assemblies.Length == 0)
                throw new ArgumentException("Assemblies length should be greater than 0", "assemblies");

            foreach (var assembly in assemblies)
            {
                if (this.assemblies.Contains(assembly))
                    throw new ArgumentException(
                        string.Format("Assembly {0} has been already registered", assembly.FullName));

                this.assemblies.Add(assembly);
            }
        }

        protected internal void Configure()
        {
            if (serializer != null)
            {
                var instance = (IMessageSerializer) Activator.CreateInstance(serializer.Item1);
                instance.Init(assemblies.ToArray(), serializer.Item2 ?? new Dictionary<string, string>());
                
                MessageEnvelope.Serializer = instance;
            }

            if (activator != null)
            {
                var instance = (IActorActivator) Activator.CreateInstance(activator.Item1);
                instance.Init(activator.Item2 ?? new Dictionary<string, string>());

                ActorEndpoint.Activator = instance;
            }

            ActorAssembly.Register(assemblies);
        }

        public void Dispose()
        {
            MessageEnvelope.Reset();
            ActorEndpoint.Reset();
            ActorAssembly.Reset();
        }
    }
}