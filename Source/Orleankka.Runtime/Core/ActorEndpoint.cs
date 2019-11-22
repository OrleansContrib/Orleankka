using System.Threading.Tasks;

using Orleans;
using Orleans.Runtime;

using Microsoft.Extensions.DependencyInjection;

namespace Orleankka.Core
{
	using Cluster;

    /// <summary> 
    /// FOR INTERNAL USE ONLY!
    /// </summary>
    public abstract class ActorEndpoint : Grain, IRemindable, IActorHost
    {
        Actor instance;
        IActorMiddleware middleware;
        ActorActivationContext ctx;

        public void Initialize(IGrainActivationContext context)
        {
            ctx = new ActorActivationContext(context, Actor.Class);

            var system = context.ActivationServices.GetRequiredService<ClusterActorSystem>();
            middleware = Actor.Middleware(system.Pipeline);

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
        public Task Notify(object message) => Receive(message);
        public Task ReceiveVoid(object message) => Receive(message);
        
        public Task<object> Receive(object message) => middleware.Receive(instance, message, instance.OnReceive);

        async Task IRemindable.ReceiveReminder(string name, TickStatus status)
        {
            await middleware.Receive(instance, new Reminder(name, status), async x =>
            {
                await instance.OnReminder(((Reminder) x).Name);
                return Done.Result;
            });
        }

        public override async Task OnActivateAsync()
        {
            await middleware.Receive(instance, Activate.Message, async _ =>
            {
                await instance.OnActivate();
                return Done.Result;
            });
        }

        public override async Task OnDeactivateAsync()
        {
            await middleware.Receive(instance, Deactivate.Message, async _ =>
            {
                await instance.OnDeactivate();
                return Done.Result;
            });
        }

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