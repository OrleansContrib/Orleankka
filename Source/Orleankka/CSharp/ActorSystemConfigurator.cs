using System;
using System.Collections.Generic;
using System.Reflection;

using Orleankka.Core;
using Orleankka.Utility;

namespace Orleankka.CSharp
{
    public static class CSharpActorSystemConfigurator
    {
        public static TConfigurator CSharp<TConfigurator>(this TConfigurator configurator, Action<CSharpActorSystemConfiguratorHook> configure) where TConfigurator : IActorSystemConfigurator
        {
            configurator.Hook<CSharpActorSystemConfiguratorHook>();

            foreach (var hook in configurator.Hooks<CSharpActorSystemConfiguratorHook>())
                configure(hook);

            return configurator;
        }
    }

    public class CSharpActorSystemConfiguratorHook : ActorSystemConfiguratorHook
    {
        readonly HashSet<Assembly> assemblies = new HashSet<Assembly>();
        Tuple<Type, object> activator;

        public CSharpActorSystemConfiguratorHook Register<T>(object properties) where T : IActorActivator
        {
            if (activator != null)
                throw new InvalidOperationException("Activator has been already registered");

            activator = Tuple.Create(typeof(T), properties);

            return this;
        }

        public CSharpActorSystemConfiguratorHook Register(params Assembly[] assemblies)
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

            return this;
        }

        protected internal override void Configure(IActorSystemConfigurator configurator)
        {
            if (activator != null)
            {
                var instance = (IActorActivator)Activator.CreateInstance(activator.Item1);
                instance.Init(activator.Item2);

                ActorBinding.Activator = instance;
            }

            configurator.Register(ActorBinding.Bind(assemblies));
        }

        public override void Dispose()
        {
            ActorBinding.Reset();
        }
    }
}