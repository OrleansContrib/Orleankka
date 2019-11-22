using System;
using System.Reflection;

using Orleans;
using Orleans.Runtime;
using System.Collections.Concurrent;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Orleans.Core;
using Orleans.Storage;

namespace Orleankka.Facets
{
    using Cluster;

    /// <summary>
    /// Provides activation of storage facets and
    /// </summary>
    public class StorageProviderFacet
    {
        readonly ConcurrentDictionary<Type, IStateStorageBridgeActivator> activators = 
             new ConcurrentDictionary<Type, IStateStorageBridgeActivator>();

        public Factory<IGrainActivationContext, IStorage<TState>> GetFactory<TState>(string provider) where TState : new() => 
            ctx => (IStorage<TState>) GetFactory(provider, typeof(TState))(ctx);

        public Factory<IGrainActivationContext, object> GetFactory(string provider, Type state)
        {
            return context =>
            {
                var activator = activators.GetOrAdd(state, t =>
                {
                    var system = context.ActivationServices.GetRequiredService<ClusterActorSystem>();
                    var storage = context.ActivationServices.GetRequiredServiceByName<IGrainStorage>(provider);
                    var loggerFactory = context.ActivationServices.GetRequiredService<ILoggerFactory>();
                    var activatorType = typeof(StateStorageBridgeActivator<>).MakeGenericType(state);
                    return (IStateStorageBridgeActivator) Activator.CreateInstance(activatorType, provider, system, storage, loggerFactory);
                });

                return activator.Activate(context);
            };
        }
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public class UseStorageProviderAttribute : Attribute, IFacetMetadata
    {
        public string Name { get; set; }

        public UseStorageProviderAttribute(string name)
        {
            Name = name;
        }
    }

    class UseStorageProviderAttributeMapper : IAttributeToFactoryMapper<UseStorageProviderAttribute>
    {
        readonly StorageProviderFacet facet;

        public UseStorageProviderAttributeMapper(IServiceProvider sp) => 
            facet = sp.GetService<StorageProviderFacet>();

        public Factory<IGrainActivationContext, object> GetFactory(ParameterInfo parameter, UseStorageProviderAttribute attribute) => 
            facet.GetFactory(attribute.Name, parameter.ParameterType.GetGenericArguments()[0]);
    }

    interface IStateStorageBridgeActivator
    {
        object Activate(IGrainActivationContext context);
    }

    class StateStorageBridgeActivator<TState> : IStateStorageBridgeActivator where TState : new()
    {
        readonly string name;
        readonly ClusterActorSystem system;
        readonly IGrainStorage storage;
        readonly ILoggerFactory loggerFactory;

        public StateStorageBridgeActivator(string name, ClusterActorSystem system, IGrainStorage storage, ILoggerFactory loggerFactory)
        {
            this.name = name;
            this.system = system;
            this.storage = storage;
            this.loggerFactory = loggerFactory;
        }

        public object Activate(IGrainActivationContext context)
        {
            var actor = system.ImplementationOf(context.GrainType);
            var actorRef = system.ActorOf(actor.Interface, context.GrainIdentity.PrimaryKeyString);

            var bridge = new StateStorageBridge<TState>(context.GrainType.FullName, actorRef, storage, loggerFactory);
            context.ObservableLifecycle.Subscribe("StorageBridge_ReadState_OnActivateAsync", GrainLifecycleStage.SetupState, _ => bridge.ReadStateAsync());

            return bridge;
        }
    }
}