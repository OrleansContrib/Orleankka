        Actor instance;
        IActorInvoker invoker;
        
        // unused
        public Task Autorun() => Task.CompletedTask;
        public Task<object> Receive(object message) => invoker.OnReceive(instance, message);
        public Task ReceiveVoid(object message) => Receive(message);
        public Task Notify(object message) => Receive(message);

        async Task IRemindable.ReceiveReminder(string name, TickStatus status) => 
            await invoker.OnReminder(instance, name);

        public override Task OnDeactivateAsync()
        {
            return instance != null
                ? invoker.OnDeactivate(instance)
                : base.OnDeactivateAsync();
        }

        public override async Task OnActivateAsync()
        {
            var @interface = Actor.Interface.Mapping.CustomInterface;
            var path = ActorPath.For(@interface, IdentityOf(this));

            var system = ServiceProvider.GetRequiredService<ClusterActorSystem>();
            var activator = ServiceProvider.GetRequiredService<IActorActivator>();

            var runtime = new ActorRuntime(system, this);

            instance = Actor.Activate(this, path, runtime, activator);
            invoker = Actor.Invoker(system.Pipeline);

            await invoker.OnActivate(instance);
        }

        public IGrainRuntime Runtime => this.Runtime();

        static string IdentityOf(IGrain grain) => 
            (grain as IGrainWithStringKey).GetPrimaryKeyString();

        protected abstract ActorType Actor { get; }