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
        IGrainActivationService,
        IGrainReminderService,
        IGrainTimerService
    {
        public static Func<Type, Actor> Activator;
            
        static ActorEndpoint()
        {
            Activator = type => (Actor) System.Activator.CreateInstance(type);
        }

        Actor actor;

        public async Task ReceiveTell(RequestEnvelope envelope)
        {
            if (actor == null)
                await Activate(envelope.Target);

            Debug.Assert(actor != null);
            await actor.OnTell(envelope.Message);
        }

        public async Task<ResponseEnvelope> ReceiveAsk(RequestEnvelope envelope)
        {
            if (actor == null)
                await Activate(envelope.Target);

            Debug.Assert(actor != null);
            return new ResponseEnvelope(await actor.OnAsk(envelope.Message));
        }

        async Task IRemindable.ReceiveReminder(string reminderName, TickStatus status)
        {
            if (actor == null)
                await Activate(ActorPath.From(IdentityOf(this)));

            Debug.Assert(actor != null);
            await actor.OnReminder(reminderName);
        }

        async Task Activate(ActorPath path)
        {
            actor = Activator(path.RuntimeType());
            actor.Initialize(path.Id, ActorSystem.Instance, this);

            await actor.OnActivate();
        }

        #region Internals

        void IGrainActivationService.DeactivateOnIdle()
        {
            DeactivateOnIdle();
        }

        void IGrainActivationService.DelayDeactivation(TimeSpan timeSpan)
        {
            DelayDeactivation(timeSpan);
        }

        Task<IGrainReminder> IGrainReminderService.GetReminder(string reminderName)
        {
            return GetReminder(reminderName);
        }

        Task<List<IGrainReminder>> IGrainReminderService.GetReminders()
        {
            return GetReminders();
        }

        Task<IGrainReminder> IGrainReminderService.RegisterOrUpdateReminder(string reminderName, TimeSpan dueTime, TimeSpan period)
        {
            return RegisterOrUpdateReminder(reminderName, dueTime, period);
        }

        Task IGrainReminderService.UnregisterReminder(IGrainReminder reminder)
        {
            return UnregisterReminder(reminder);
        }

        IDisposable IGrainTimerService.RegisterTimer(Func<object, Task> asyncCallback, object state, TimeSpan dueTime, TimeSpan period)
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