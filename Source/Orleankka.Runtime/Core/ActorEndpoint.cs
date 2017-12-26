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
    public abstract class ActorEndpoint : Grain, IRemindable, IActorHost
    {
        const string StickyReminderName = "##sticky##";

        Actor instance;
        IActorInvoker invoker;

        ActorType actor;
        ActorType Actor => actor ?? (actor = ActorType.OfGrain(GetType()));

        public Task Autorun()
        {
            KeepAlive();

            return Task.CompletedTask;
        }

        public Task<object> Receive(object message)
        {
            KeepAlive();

            return invoker.OnReceive(instance, message);
        }

        public Task ReceiveVoid(object message) => Receive(message);

        public Task Notify(object message) => Receive(message);

        async Task IRemindable.ReceiveReminder(string name, TickStatus status)
        {
            KeepAlive();

            if (name == StickyReminderName)
                return;

            await invoker.OnReminder(instance, name);
        }

        public override Task OnDeactivateAsync()
        {
            return instance != null
                ? invoker.OnDeactivate(instance)
                : base.OnDeactivateAsync();
        }

        async Task HandleStickyness()
        {
            var period = TimeSpan.FromMinutes(1);
            await RegisterOrUpdateReminder(StickyReminderName, period, period);
        }

        void KeepAlive() => Actor.KeepAlive(this);

        public override async Task OnActivateAsync()
        {
            if (Actor.Sticky)
                await HandleStickyness();

            await Activate();
        }

        Task Activate()
        {
            var path = ActorPath.From(Actor.Name, IdentityOf(this));

            var system = ServiceProvider.GetRequiredService<ClusterActorSystem>();
            var activator = ServiceProvider.GetRequiredService<IActorActivator>();

            var runtime = new ActorRuntime(system, this);

            instance = Actor.Activate(this, path, runtime, activator);
            invoker = Actor.Invoker(system.Pipeline);

            return invoker.OnActivate(instance);
        }

        public IGrainRuntime Runtime => this.Runtime();

        static string IdentityOf(IGrain grain) => 
            (grain as IGrainWithStringKey).GetPrimaryKeyString();
    }
}