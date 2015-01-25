using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Orleans;
using Orleans.Runtime;

namespace Orleankka
{
    public abstract class Actor : Grain, IActor, IActorObserver,
        IInternalActivationService, 
        IInternalReminderService,
        IInternalTimerService 
    {
        string id;
        ActorPath self;
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

        public ActorPath Self
        {
            get { return (self ?? (self = new ActorPath(ActorInterface.Of(GetType()), Id))); }
        }

        public string Id
        {
            get { return (id ?? (id = Identity.Of((IActor)this))); }
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

        public virtual void OnNext(Notification notification)
        {
            throw NotImplemented("OnNext");
        }

        Task IRemindable.ReceiveReminder(string reminderName, TickStatus status)
        {
            return OnReminder(reminderName);
        }

        NotImplementedException NotImplemented(string method)
        {
            return new NotImplementedException(string.Format(
                "Override {0}() method in class {1} to implement corresponding behavior", 
                method, GetType())
            );
        }

        protected IActorRef ActorOf(ActorPath path)
        {
            return System.ActorOf(path);
        }        
        
        protected IActorObserver ObserverOf(ActorPath path)
        {
            return System.ObserverOf(path);
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
