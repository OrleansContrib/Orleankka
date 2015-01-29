using System;
using System.Threading.Tasks;

using Orleans;

namespace Orleankka
{
    using Internal;

    public abstract class Actor
    {
        string id;
        ActorRef self;
        IActorSystem system;
        
        protected Actor()
        {}

        protected Actor(string id, IActorSystem system)
        {
            Requires.NotNull(system, "system");
            Requires.NotNullOrWhitespace(id, "id");

            this.id = id;
            this.system = system;
        }

        internal void Initialize(ActorHost host, string id, IActorSystem system)
        {
            Host = host;
            this.id = id;
            this.system = system;
        }

        internal ActorHost Host
        {
            get; private set;
        }

        public ActorRef Self
        {
            get { return (self ?? (self = ActorOf(ActorPath.From(GetType(), Id)))); }
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

        public static implicit operator ActorPath(Actor arg)
        {
            return arg.Self;
        }

        internal static bool IsCompatible(Type type)
        {
            return typeof(Actor).IsAssignableFrom(type) && !type.IsAbstract;
        }

        public static IActorObserver Observer(ActorPath path)
        {
            return ActorObserverFactory.Cast(Factory.Create(path));
        }

        internal static IActorProxy Proxy(ActorPath path)
        {
            return new ActorProxy(Factory.Create(path), path);
        }

        static class Factory
        {
            public static IActorHost Create(ActorPath path)
            {
                return ActorHostFactory.GetGrain(path.ToString());
            }
        }
    }
}