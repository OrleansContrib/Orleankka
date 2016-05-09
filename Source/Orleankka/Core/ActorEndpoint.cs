using System;
using System.Collections.Generic;
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
    /// Generic actor endpoint.
    /// </summary>
    public abstract class ActorEndpoint<T> : ActorEndpoint where T : Actor
    {}

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

        Actor instance;

        public async Task<ResponseEnvelope> Receive(RequestEnvelope envelope)
        {
            KeepAlive();

            return new ResponseEnvelope(await instance.OnReceive(envelope.Message));
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

            return instance.OnReceive(envelope.Message);
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

            await instance.OnReminder(reminderName);
        }

        public override Task OnActivateAsync()
        {
            return Activate(ActorPath.Deserialize(IdentityOf(this)));
        }

        public override Task OnDeactivateAsync()
        {
            return instance != null
                    ? instance.OnDeactivate()
                    : base.OnDeactivateAsync();
        }

        Task Activate(ActorPath path)
        {
            var actor = path.Type();

            var system = ClusterActorSystem.Current;
            var runtime = new ActorRuntime(system, this);

            instance = Activator.Activate(actor.Implementation.Type, path.Id, runtime);
            instance.Initialize(actor, path.Id, runtime);

            return instance.OnActivate();
        }

        void KeepAlive()
        {
            instance.Implementation.KeepAlive(this);
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

    namespace Endpoints
    {
        /// <summary>
        ///   FOR INTERNAL USE ONLY!
        ///   Fixed grain endpoint with Placement.Random
        /// </summary>
        public class A0 : ActorEndpoint, IA0
        {}

        /// <summary>
        ///   FOR INTERNAL USE ONLY!
        ///   Fixed grain endpoint with Placement.PreferLocal
        /// </summary>
        [PreferLocalPlacement]
        public class A1 : ActorEndpoint, IA1
        {}

        /// <summary>
        ///   FOR INTERNAL USE ONLY!
        ///   Fixed grain endpoint with Placement.DistributeEvenly
        /// </summary>
        [ActivationCountBasedPlacement]
        public class A2 : ActorEndpoint, IA2
        {}

        /// <summary>
        ///   FOR INTERNAL USE ONLY!
        ///   Fixed worker grain endpoint
        /// </summary>
        [StatelessWorker]
        public class W : ActorEndpoint, IW
        {}
    }
}