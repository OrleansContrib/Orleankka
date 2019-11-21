using System;
using System.Threading.Tasks;

using Orleans;
using Orleans.Runtime;
using Orleans.Internals;

using Microsoft.Extensions.DependencyInjection;

namespace Orleankka.Core
{
	using Cluster;

    /// <summary> 
    /// FOR INTERNAL USE ONLY!
    /// </summary>
    public abstract class ActorEndpoint : Grain, IRemindable, IActorHost, IActorLifecycleManager
    {
        Actor instance;
        IActorInvoker invoker;
        ActorActivationContext ctx;

        public void Initialize(IGrainActivationContext context)
        {
            ctx = new ActorActivationContext(context, Actor.Class);

            var system = context.ActivationServices.GetRequiredService<ClusterActorSystem>();
            invoker = Actor.Invoker(system.Pipeline);

            var @interface = Actor.Interface.Mapping.CustomInterface;
            var path = ActorPath.For(@interface, context.GrainIdentity.PrimaryKeyString);
            var runtime = new ActorRuntime(system, this);
                    
            var activator = context.ActivationServices.GetRequiredService<IGrainActivator>();
            instance = (Actor) activator.Create(ctx);
            instance.Initialize(this, path, runtime, Actor.dispatcher);
        }

        public void Release(IGrainActivationContext context)
        {
            var activator = context.ActivationServices.GetRequiredService<IGrainActivator>();
            activator.Release(ctx, instance);
        }

        // unused
        public Task Autorun() => Task.CompletedTask;
        public Task<object> Receive(object message) => invoker.OnReceive(instance, message);
        public Task ReceiveVoid(object message) => Receive(message);
        public Task Notify(object message) => Receive(message);

        async Task IRemindable.ReceiveReminder(string name, TickStatus status) => 
        await invoker.OnReminder(instance, name);

        public override Task OnDeactivateAsync() => invoker.OnDeactivate(instance);
        public override Task OnActivateAsync() => invoker.OnActivate(instance);

        protected abstract ActorType Actor { get; }  
    }

    /// <summary> 
    /// FOR INTERNAL USE ONLY!
    /// </summary>
    public abstract class ActorEndpoint<TInterface> : ActorEndpoint
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