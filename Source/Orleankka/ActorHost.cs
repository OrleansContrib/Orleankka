using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Orleans;
using Orleans.Runtime;

namespace Orleankka
{
    namespace Internal
    {
        /// <summary> 
        /// FOR INTERNAL USE ONLY! 
        /// </summary>
        public class ActorHost : Grain, IActorHost, IActorObserver,
            IInternalActivationService,
            IInternalReminderService,
            IInternalTimerService
        {
            public static Func<Type, Actor> Activator;
            
            static ActorHost()
            {
                Activator = type => (Actor) System.Activator.CreateInstance(type);
            }

            Actor actor;

            public async Task ReceiveTell(Request request)
            {
                if (actor == null)
                    await Activate(request.Target);

                Debug.Assert(actor != null);
                await actor.OnTell(request.Message);
            }

            public async Task<Response> ReceiveAsk(Request request)
            {
                if (actor == null)
                    await Activate(request.Target);

                Debug.Assert(actor != null);
                return new Response(await actor.OnAsk(request.Message));
            }

            public void OnNext(Notification notification)
            {
                Debug.Assert(actor != null);
                actor.OnNext(notification);
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
                actor = Activator(ActorSystem.RuntimeType(path));
                actor.Initialize(this, path.Id, ActorSystem.Instance);

                await actor.OnActivate();
            }

            static string IdentityOf(IGrain grain)
            {
                string identity;
                grain.GetPrimaryKeyLong(out identity);
                return identity;
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
        }
    }
}