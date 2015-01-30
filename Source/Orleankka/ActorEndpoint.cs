using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Orleans;
using Orleans.Runtime;

namespace Orleankka
{
    namespace Core
    {
        /// <summary> 
        /// FOR INTERNAL USE ONLY! 
        /// </summary>
        public class ActorEndpoint : Grain, 
            IActorEndpoint,
            IInternalActivationService,
            IInternalReminderService,
            IInternalTimerService
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
                actor.Initialize(this, path.Id, ActorSystem.Instance);

                await actor.OnActivate();
            }

            #region Internals

            void IInternalActivationService.DeactivateOnIdle()
            {
                DeactivateOnIdle();
            }

            void IInternalActivationService.DelayDeactivation(TimeSpan timeSpan)
            {
                DelayDeactivation(timeSpan);
            }

            Task<IGrainReminder> IInternalReminderService.GetReminder(string reminderName)
            {
                return GetReminder(reminderName);
            }

            Task<List<IGrainReminder>> IInternalReminderService.GetReminders()
            {
                return GetReminders();
            }

            Task<IGrainReminder> IInternalReminderService.RegisterOrUpdateReminder(string reminderName, TimeSpan dueTime, TimeSpan period)
            {
                return RegisterOrUpdateReminder(reminderName, dueTime, period);
            }

            Task IInternalReminderService.UnregisterReminder(IGrainReminder reminder)
            {
                return UnregisterReminder(reminder);
            }

            IDisposable IInternalTimerService.RegisterTimer(Func<object, Task> asyncCallback, object state, TimeSpan dueTime, TimeSpan period)
            {
                return RegisterTimer(asyncCallback, state, dueTime, period);
            }

            #endregion

            internal static string IdentityOf(IGrain grain)
            {
                string identity;
                grain.GetPrimaryKeyLong(out identity);
                return identity;
            }

            internal static IActorEndpoint Proxy(ActorPath path)
            {
                return Factory.Create(path);
            }

            static class Factory
            {
                public static IActorEndpoint Create(ActorPath path)
                {
                    return ActorEndpointFactory.GetGrain(path.ToString());
                }
            }
        }
    }
}