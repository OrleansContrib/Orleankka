using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Orleans;
using Orleans.Core;
using Orleans.Runtime;
using Orleans.Internals;

using Microsoft.Extensions.DependencyInjection;

namespace Orleankka.Core
{
	using Cluster;

    /// <summary> 
    /// FOR INTERNAL USE ONLY!
    /// </summary>
    public abstract class StatefulActorEndpoint<TState> : Grain<TState>, IRemindable, IActorHost where TState : new()
    {
        Actor instance;
        IActorInvoker invoker;
        
        // unused
        public Task Autorun() => Task.CompletedTask;
        public Task<object> Receive(object message) => invoker.OnReceive(instance, message);
        public Task ReceiveVoid(object message) => Receive(message);
        public Task Notify(object message) => Receive(message);

        async Task IRemindable.ReceiveReminder(string name, TickStatus status) => 
            await invoker.OnReminder(instance, name);

        public override Task OnDeactivateAsync()
        {
            return instance != null
                ? invoker.OnDeactivate(instance)
                : base.OnDeactivateAsync();
        }

        public override async Task OnActivateAsync()
        {
            var @interface = Actor.Interface.Mapping.CustomInterface;
            var path = ActorPath.For(@interface, IdentityOf(this));

            var system = ServiceProvider.GetRequiredService<ClusterActorSystem>();
            var activator = ServiceProvider.GetRequiredService<IActorActivator>();

            var runtime = new ActorRuntime(system, this);

            instance = Actor.Activate(this, path, runtime, activator);
            invoker = Actor.Invoker(system.Pipeline);

            await invoker.OnActivate(instance);
        }

        public IGrainRuntime Runtime => this.Runtime();

        static string IdentityOf(IGrain grain) => 
            (grain as IGrainWithStringKey).GetPrimaryKeyString();

        protected abstract ActorType Actor { get; }

        public new TState State
        {
            get { return base.State; }
            set { base.State = value; }
        }

        public new Task ClearStateAsync()
        {
            return base.ClearStateAsync();
        }

        public new Task WriteStateAsync()
        {
            return base.WriteStateAsync();
        }

        public new Task ReadStateAsync()
        {
            return base.ReadStateAsync();
        }  
    }

    /// <summary> 
    /// FOR INTERNAL USE ONLY!
    /// </summary>
    public abstract class StatefulActorEndpoint<TInterface, TState> : StatefulActorEndpoint<TState> where TState : new()
    {
        #pragma warning disable 649
        // ReSharper disable once StaticMemberInGenericType
        // ReSharper disable once UnassignedField.Global
        // ReSharper disable once MemberCanBePrivate.Global
        protected static ActorType type;
        #pragma warning restore 649

        protected override ActorType Actor => type;  
    }
}