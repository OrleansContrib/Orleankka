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
        Tuple<Type, object> activator;

        protected void RegisterActivator<T>(object properties) where T : IActorActivator
        {
            if (activator != null)
                throw new InvalidOperationException("Activator has been already registered");

            activator = Tuple.Create(typeof(T), properties);
        }

        protected void RegisterAssemblies(params Assembly[] assemblies)
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

        protected void Configure()
        {
            if (activator != null)
            {
                var instance = (IActorActivator) Activator.CreateInstance(activator.Item1);
                instance.Init(activator.Item2);

                ActorEndpoint.Activator = instance;
            }

            ActorType.Register(assemblies.ToArray());
        }

        public void Dispose()
        {
            ActorEndpoint.Reset();
            ActorType.Reset();
        }
        
        public override object InitializeLifetimeService()
        {
            return null;
        }
    }
}