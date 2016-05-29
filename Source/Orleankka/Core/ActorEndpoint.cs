using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;

using Orleans;
using Orleans.Runtime;

namespace Orleankka.Core
{
    using Cluster;

    /// <summary> 
    /// FOR INTERNAL USE ONLY!
    /// </summary>
    public abstract class ActorEndpoint : Grain, IRemindable
    {
        internal static IActorActivator Activator;

        internal static void Reset()
        {
            Activator = new DefaultActorActivator();
        }

        static ActorEndpoint()
        {
            Reset();
        }

        readonly ActorType type;
        Actor actor;

        protected ActorEndpoint(string code)
        {
            this.type = ActorType.Registered(code);
        }

        public async Task<ResponseEnvelope> Receive(RequestEnvelope envelope)
        {
            KeepAlive();

            return new ResponseEnvelope(await actor.OnReceive(envelope.Message));
        }

        public Task<ResponseEnvelope> ReceiveReentrant(RequestEnvelope envelope)
        {
            #if DEBUG
                CallContext.LogicalSetData("LastMessageReceivedReentrant", envelope.Message);
            #endif

            return Receive(envelope);
        }

        public Task ReceiveVoid(RequestEnvelope envelope)
        {
            KeepAlive();

            return actor.OnReceive(envelope.Message);
        }

        public Task ReceiveReentrantVoid(RequestEnvelope envelope)
        {
            #if DEBUG
                CallContext.LogicalSetData("LastMessageReceivedReentrantVoid", envelope.Message);
            #endif

            return ReceiveVoid(envelope);
        }

        async Task IRemindable.ReceiveReminder(string reminderName, TickStatus status)
        {
            KeepAlive();

            await actor.OnReminder(reminderName);
        }

        public override Task OnActivateAsync()
        {
            return Activate(ActorPath.Deserialize(IdentityOf(this)));
        }

        public override Task OnDeactivateAsync()
        {
            return actor != null
                    ? actor.OnDeactivate()
                    : base.OnDeactivateAsync();
        }

        Task Activate(ActorPath path)
        {
            var system = ClusterActorSystem.Current;
            var runtime = new ActorRuntime(system, this);

            actor = Activator.Activate(type.Implementation.Type, path.Id, runtime);
            actor.Initialize(type, path.Id, runtime);

            return actor.OnActivate();
        }

        void KeepAlive()
        {
            actor.Implementation.KeepAlive(this);
        }

        #region Internals

        internal new void DeactivateOnIdle()
        {
            base.DeactivateOnIdle();
        }

        internal new void DelayDeactivation(TimeSpan timeSpan)
        {
            base.DelayDeactivation(timeSpan);
        }

        internal new Task<IGrainReminder> GetReminder(string reminderName)
        {
            return base.GetReminder(reminderName);
        }

        internal new Task<List<IGrainReminder>> GetReminders()
        {
            return base.GetReminders();
        }

        internal new Task<IGrainReminder> RegisterOrUpdateReminder(string reminderName, TimeSpan dueTime, TimeSpan period)
        {
            return base.RegisterOrUpdateReminder(reminderName, dueTime, period);
        }

        internal new Task UnregisterReminder(IGrainReminder reminder)
        {
            return base.UnregisterReminder(reminder);
        }

        internal new IDisposable RegisterTimer(Func<object, Task> asyncCallback, object state, TimeSpan dueTime, TimeSpan period)
        {
            return base.RegisterTimer(asyncCallback, state, dueTime, period);
        }

        #endregion

        static string IdentityOf(IGrain grain)
        {
            return (grain as IGrainWithStringKey).GetPrimaryKeyString();
        }
    }
}