using System;
using System.Collections.Generic;
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
        public class Host
            : Grain, IActor, IActorObserver,
              IInternalActivationService,
              IInternalReminderService,
              IInternalTimerService
        {
            Actor actor;

            public async Task OnTell(Request request)
            {
                await EnsureInstance(request.Target);
                await actor.OnTell(request.Message);
            }

            public async Task<Response> OnAsk(Request request)
            {
                await EnsureInstance(request.Target);
                return new Response(await actor.OnAsk(request.Message));
            }

            public void OnNext(Notification notification)
            {
                actor.OnNext(notification);
            }

            Task IRemindable.ReceiveReminder(string reminderName, TickStatus status)
            {
                throw new NotImplementedException("TODO: Parse type and id from actor id");
            }

            async Task EnsureInstance(ActorPath path)
            {
                if (actor != null)
                    return;

                actor = ActorSystem.Activator(path);
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
        }
    }
}