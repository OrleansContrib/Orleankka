﻿        const string StickyReminderName = "##sticky##";

        Actor instance;

        public Task Autorun()
        {
            KeepAlive();

            return TaskDone.Done;
        }

        public Task<object> Receive(object message)
        {
            KeepAlive();

            return Actor.Invoker.OnReceive(instance, message);
        }

        public Task ReceiveVoid(object message) => Receive(message);

        async Task IRemindable.ReceiveReminder(string name, TickStatus status)
        {
            KeepAlive();

            if (name == StickyReminderName)
                return;

            await Actor.Invoker.OnReminder(instance, name);
        }

        public override Task OnDeactivateAsync()
        {
            return instance != null
                ? Actor.Invoker.OnDeactivate(instance)
                : base.OnDeactivateAsync();
        }

        async Task HandleStickyness()
        {
            var period = TimeSpan.FromMinutes(1);
            await RegisterOrUpdateReminder(StickyReminderName, period, period);
        }

        void KeepAlive() => Actor.KeepAlive(this);

        public override async Task OnActivateAsync()
        {
            if (Actor.Sticky)
                await HandleStickyness();

            await Activate();
        }

        Task Activate()
        {
            var path = ActorPath.From(Actor.Name, IdentityOf(this));
            var runtime = new ActorRuntime(ClusterActorSystem.Current, this);
            instance = Actor.Activate(this, path, runtime);
            return Actor.Invoker.OnActivate(instance);
        }

        static string IdentityOf(IGrain grain)
        {
            return (grain as IGrainWithStringKey).GetPrimaryKeyString();
        }

        protected abstract ActorType Actor { get; }

        #region Expose protected methods to actor services layer

        public new void DeactivateOnIdle()
        {
            base.DeactivateOnIdle();
        }

        public new void DelayDeactivation(TimeSpan timeSpan)
        {
            base.DelayDeactivation(timeSpan);
        }

        public new Task<IGrainReminder> GetReminder(string reminderName)
        {
            return base.GetReminder(reminderName);
        }

        public new Task<List<IGrainReminder>> GetReminders()
        {
            return base.GetReminders();
        }

        public new Task<IGrainReminder> RegisterOrUpdateReminder(string reminderName, TimeSpan dueTime, TimeSpan period)
        {
            return base.RegisterOrUpdateReminder(reminderName, dueTime, period);
        }

        public new Task UnregisterReminder(IGrainReminder reminder)
        {
            return base.UnregisterReminder(reminder);
        }

        public new IDisposable RegisterTimer(Func<object, Task> asyncCallback, object state, TimeSpan dueTime, TimeSpan period)
        {
            return base.RegisterTimer(asyncCallback, state, dueTime, period);
        }

        #endregion