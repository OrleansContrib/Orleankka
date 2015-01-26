using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Orleans;
using Orleans.Runtime;

namespace Orleankka
{
    namespace Dynamic
    {
        /// <summary> 
        /// FOR INTERNAL USE ONLY! 
        /// </summary>
        public class DynamicActorHost : Grain, IDynamicActor, IDynamicActorObserver,
            IInternalActivationService, 
            IInternalReminderService,
            IInternalTimerService 
        {
            DynamicActor actor;

            public async Task OnTell(DynamicRequest request)
            {
                await EnsureInstance(request.Target);
                await actor.OnTell(request.Message);
            }

            public async Task<DynamicResponse> OnAsk(DynamicRequest request)
            {
                await EnsureInstance(request.Target);
                return new DynamicResponse(await actor.OnAsk(request.Message));
            }

            public void OnNext(DynamicNotification notification)
            {
                actor.OnNext(new Notification(notification.Source, notification.Message));
            }

            Task IRemindable.ReceiveReminder(string reminderName, TickStatus status)
            {
                throw new NotImplementedException("TODO: Parse type and id from actor id");
            }

            async Task EnsureInstance(ActorPath path)
            {
                if (actor != null)
                    return;

                actor = ActorSystem.Dynamic.Activator(path);
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
