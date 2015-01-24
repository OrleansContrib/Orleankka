using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Orleans;
using Orleans.Runtime;

namespace Orleankka
{
    public abstract class Actor : Grain, IActor, 
        IInternalActivationService, 
        IInternalReminderService,
        IInternalTimerService 
    {
        string id;
        ActorPath path;
        readonly IActorSystem system;

        protected Actor()
        {
            system = ActorSystem.Instance;
        }
        
        protected Actor(string id, IActorSystem system)
        {
            Requires.NotNull(system, "system");
            Requires.NotNullOrWhitespace(id, "id");

            this.id = id;
            this.system = system;
        }

        public ActorPath Path
        {
            get { return (path ?? (path = ActorPath.Map(GetType(), Id))); }
        }

        public string Id
        {
            get { return (id ?? (id = Identity.Of(this))); }
        }

        public IActorRef Self()
        {
            return system.ActorOf(Path);
        }

        public IActorSystem System
        {
            get { return system; }
        }

        public virtual Task OnTell(object message)
        {
            throw NotImplemented("OnTell");
        }

        public virtual Task<object> OnAsk(object message)
        {
            throw NotImplemented("OnAsk");
        }

        public virtual Task OnReminder(string id)
        {
            throw NotImplemented("OnReminder");
        }

        Task IRemindable.ReceiveReminder(string reminderName, TickStatus status)
        {
            return OnReminder(reminderName);
        }

        protected Notification Notification(object message)
        {
            return new Notification(Path, message);
        }

        NotImplementedException NotImplemented(string method)
        {
            return new NotImplementedException(string.Format(
                "Override {0}() method in class {1} to implement corresponding behavior", 
                method, GetType())
            );
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
