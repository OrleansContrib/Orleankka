using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;

using Orleans;
using Orleans.Concurrency;
using Orleans.Placement;
using Orleans.Runtime;

namespace Orleankka.Core
{
    using Cluster;

    /// <summary> 
    /// FOR INTERNAL USE ONLY!
    /// </summary>
    public abstract class ActorEndpoint : Grain,
        IRemindable,
        IActorEndpointActivationService,
        IActorEndpointReminderService,
        IActorEndpointTimerService
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

        Actor actor;

        public async Task<ResponseEnvelope> Receive(RequestEnvelope envelope)
        {
            if (actor == null)
                await Activate(ActorPath.Deserialize(envelope.Target));

            Debug.Assert(actor != null);
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

        async Task IRemindable.ReceiveReminder(string reminderName, TickStatus status)
        {
            if (actor == null)
                await Activate(ActorPath.Deserialize(IdentityOf(this)));

            Debug.Assert(actor != null);
            KeepAlive();

            await actor.OnReminder(reminderName);
        }

        async Task Activate(ActorPath path)
        {
            var system = ClusterActorSystem.Current;

            actor = Activator.Activate(path.Type);
            actor.Initialize(path.Id, system, this, ActorPrototype.Of(path.Type));
            
            await actor.OnActivate();
        }

        public override Task OnDeactivateAsync()
        {
            if (actor != null)
                actor.OnDeactivate();

            return TaskDone.Done;
        }

        void KeepAlive()
        {
            actor.Prototype.KeepAlive(this);
        }

        #region Internals

        void IActorEndpointActivationService.DeactivateOnIdle()
        {
            DeactivateOnIdle();
        }

        void IActorEndpointActivationService.DelayDeactivation(TimeSpan timeSpan)
        {
            DelayDeactivation(timeSpan);
        }

        Task<IGrainReminder> IActorEndpointReminderService.GetReminder(string reminderName)
        {
            return GetReminder(reminderName);
        }

        Task<List<IGrainReminder>> IActorEndpointReminderService.GetReminders()
        {
            return GetReminders();
        }

        Task<IGrainReminder> IActorEndpointReminderService.RegisterOrUpdateReminder(string reminderName, TimeSpan dueTime, TimeSpan period)
        {
            return RegisterOrUpdateReminder(reminderName, dueTime, period);
        }

        Task IActorEndpointReminderService.UnregisterReminder(IGrainReminder reminder)
        {
            return UnregisterReminder(reminder);
        }

        IDisposable IActorEndpointTimerService.RegisterTimer(Func<object, Task> asyncCallback, object state, TimeSpan dueTime, TimeSpan period)
        {
            return RegisterTimer(asyncCallback, state, dueTime, period);
        }

        #endregion

        static string IdentityOf(IGrain grain)
        {
            return (grain as IGrainWithStringKey).GetPrimaryKeyString();
        }

        internal static IActorEndpoint Proxy(ActorPath path)
        {
            return ActorEndpointDynamicFactory.Proxy(path);
        }
    }

    namespace Static
    {
        /// <summary>
        ///   FOR INTERNAL USE ONLY!
        ///   Actor with Placement.Random
        /// </summary>
        public class A0 : ActorEndpoint, IA0
        {}

        /// <summary>
        ///   FOR INTERNAL USE ONLY!
        ///   Actor with Placement.PreferLocal
        /// </summary>
        [PreferLocalPlacement]
        public class A1 : ActorEndpoint, IA1
        {}

        /// <summary>
        ///   FOR INTERNAL USE ONLY!
        ///   Actor with Placement.DistributeEvenly
        /// </summary>
        [ActivationCountBasedPlacement]
        public class A2 : ActorEndpoint, IA2
        {}

        /// <summary>
        ///   FOR INTERNAL USE ONLY!
        ///   Worker
        /// </summary>
        [StatelessWorker]
        public class W : ActorEndpoint, IW
        {}
    }
}