using System;
using System.Threading.Tasks;

using Orleans;
using Orleankka.Dynamic.Internal;

namespace Orleankka.Dynamic
{
    public abstract class DynamicActor
    {
        string id;
        ActorRef self;
        IActorSystem system;
        
        protected DynamicActor()
        {}

        protected DynamicActor(string id, IActorSystem system)
        {
            Requires.NotNull(system, "system");
            Requires.NotNullOrWhitespace(id, "id");

            this.id = id;
            this.system = system;
        }

        internal void Initialize(DynamicActorHost host, string id, IActorSystem system)
        {
            Host = host;
            this.id = id;
            this.system = system;
        }

        internal DynamicActorHost Host
        {
            get; private set;
        }

        public ActorRef Self
        {
            get { return (self ?? (self = ActorOf(new ActorPath(GetType(), Id)))); }
        }

        public string Id
        {
            get { return id; }
        }

        public IActorSystem System
        {
            get { return system; }
        }

        public virtual Task OnActivate()
        {
            return TaskDone.Done;
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

        NotImplementedException NotImplemented(string method)
        {
            return new NotImplementedException(String.Format(
                "Override {0}() method in class {1} to implement corresponding behavior", 
                method, GetType())
            );
        }

        protected ActorRef ActorOf(ActorPath path)
        {
            return System.ActorOf(path);
        }        
        
        protected IActorObserver ObserverOf(ActorPath path)
        {
            return System.ObserverOf(path);
        }

        public static implicit operator ActorPath(DynamicActor arg)
        {
            return arg.Self;
        }

        internal static bool IsCompatible(Type type)
        {
            return typeof(DynamicActor).IsAssignableFrom(type) && !type.IsAbstract;
        }

        public static IActorObserver Observer(ActorPath path)
        {
            return new DynamicActorObserver(DynamicActorObserverFactory.Cast(Factory.Create(path)));
        }

        internal static IActorProxy Proxy(ActorPath path)
        {
            return new DynamicActorProxy(Factory.Create(path), path);
        }

        static class Factory
        {
            public static IDynamicActor Create(ActorPath path)
            {
                return DynamicActorFactory.GetGrain(path.ToString());
            }
        }
    }
}