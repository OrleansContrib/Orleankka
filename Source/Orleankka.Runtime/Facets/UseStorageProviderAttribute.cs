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
    
    [AttributeUsage(AttributeTargets.Parameter)]
    public class UseStorageProviderAttribute : Attribute, IFacetMetadata
    {
        public string Name { get; set; }

        public UseStorageProviderAttribute(string name)
        {
            Name = name;
        }
    }

    internal class UseStorageProviderAttributeMapper : IAttributeToFactoryMapper<UseStorageProviderAttribute>
    {
        readonly ConcurrentDictionary<Type, IStateStorageBridgeActivator> activators = 
             new ConcurrentDictionary<Type, IStateStorageBridgeActivator>();

        public Factory<IGrainActivationContext, object> GetFactory(ParameterInfo parameter, UseStorageProviderAttribute attribute)
        {
            return context =>
            {
                var state = parameter.ParameterType.GetGenericArguments()[0];

                var activator = activators.GetOrAdd(state, t =>
                {
                    var system = context.ActivationServices.GetRequiredService<ClusterActorSystem>();
                    var storage = context.ActivationServices.GetRequiredServiceByName<IGrainStorage>(attribute.Name);
                    var loggerFactory = context.ActivationServices.GetRequiredService<ILoggerFactory>();
                    var activatorType = typeof(StateStorageBridgeActivator<>).MakeGenericType(state);
                    return (IStateStorageBridgeActivator) Activator.CreateInstance(activatorType, attribute.Name, system, storage, loggerFactory);
                });

                return activator.Activate(context);
            };
        }
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

            var bridge = new StateStorageBridge<TState>(name, actorRef, storage, loggerFactory);
            context.ObservableLifecycle.Subscribe("StorageBridge_ReadState_OnActivateAsync", GrainLifecycleStage.SetupState, _ => bridge.ReadStateAsync());

            return bridge;
        }
    }
}