using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Orleans;
using Orleans.Core;
using Orleans.Runtime;

using Microsoft.Extensions.DependencyInjection;

namespace Orleankka.Core
{
	using Cluster;

    /// <summary> 
    /// FOR INTERNAL USE ONLY!
    /// </summary>
    public abstract class StatefulActorEndpoint<TState> : Grain<TState>, IRemindable, IActorHost where TState : new()
    {
        const string StickyReminderName = "##sticky##";

        Actor instance;
        IActorInvoker invoker;

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
            var runtime = new ActorRuntime(system, this);

            instance = Actor.Activate(this, path, runtime, system.Activator);
            invoker = Actor.Invoker(system.Pipeline);

            return invoker.OnActivate(instance);
        }

        static string IdentityOf(IGrain grain) => 
            (grain as IGrainWithStringKey).GetPrimaryKeyString();

        protected abstract ActorType Actor { get; }

        #region Expose protected methods

        public new IServiceProvider ServiceProvider => base.ServiceProvider;
        public IGrainIdentity Identity => this.GetGrainIdentity();
        public new string IdentityString => base.IdentityString;
        public new IGrainFactory GrainFactory => base.GrainFactory;
        public Logger Logger() => base.GetLogger();

        public new void DeactivateOnIdle()
        {
            base.DeactivateOnIdle();
        }

        public new void DelayDeactivation(TimeSpan timeSpan)
        {
            base.DelayDeactivation(timeSpan);
        }

        public new Task<IGrainReminder> GetReminder(string reminderName)
        {
            return base.GetReminder(reminderName);
        }

        public new Task<List<IGrainReminder>> GetReminders()
        {
            return base.GetReminders();
        }

        public new Task<IGrainReminder> RegisterOrUpdateReminder(string reminderName, TimeSpan dueTime, TimeSpan period)
        {
            return base.RegisterOrUpdateReminder(reminderName, dueTime, period);
        }

        public new Task UnregisterReminder(IGrainReminder reminder)
        {
            return base.UnregisterReminder(reminder);
        }

        public new IDisposable RegisterTimer(Func<object, Task> asyncCallback, object state, TimeSpan dueTime, TimeSpan period)
        {
            return base.RegisterTimer(asyncCallback, state, dueTime, period);
        }

        #endregion

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