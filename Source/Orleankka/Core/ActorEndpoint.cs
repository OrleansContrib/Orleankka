using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Orleans;
using Orleans.Runtime;

namespace Orleankka.Core
{
    /// <summary> 
    /// FOR INTERNAL USE ONLY!
    /// </summary>
    public abstract class ActorEndpoint : Grain,
        IRemindable,
        IActorEndpointActivationService,
        IActorEndpointReminderService,
        IActorEndpointTimerService
    {
        internal static IInstanceActivator Activator;

        internal static void Reset()
        {
            Activator = new DefaultInstanceActivator();
        }

        static ActorEndpoint()
        {
            Reset();
        }

        Actor actor;

        public async Task ReceiveTell(RequestEnvelope envelope)
        {
            if (actor == null)
                await Activate(ActorPath.Deserialize(envelope.Target));

            Debug.Assert(actor != null);
            await actor.OnTell(envelope.Message);
        }

        public async Task<ResponseEnvelope> ReceiveAsk(RequestEnvelope envelope)
        {
            if (actor == null)
                await Activate(ActorPath.Deserialize(envelope.Target));

            Debug.Assert(actor != null);
            return new ResponseEnvelope(await actor.OnAsk(envelope.Message));
        }

        async Task IRemindable.ReceiveReminder(string reminderName, TickStatus status)
        {
            if (actor == null)
                await Activate(ActorPath.Deserialize(IdentityOf(this)));

            Debug.Assert(actor != null);
            await actor.OnReminder(reminderName);
        }

        async Task Activate(ActorPath path)
        {
            var system = ActorSystem.Instance;

            actor = Activator.Activate(path.RuntimeType());
            actor.Initialize(path.Id, system, this);
            
            await actor.OnActivate();
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
            string identity;
            grain.GetPrimaryKeyLong(out identity);
            return identity;
        }

        internal static ActorEndpointInvoker Invoker(ActorPath path)
        {
            return ActorEndpointFactory.Invoker(path.RuntimeType());
        }
    }
}