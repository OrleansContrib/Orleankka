using System;
using System.Linq;
using System.Threading.Tasks;

using Orleans;

namespace Orleankka
{
    using Dynamic;

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

        class DynamicActorObserver : IActorObserver, IEquatable<DynamicActorObserver>
        {
            readonly IDynamicActorObserver observer;

            public DynamicActorObserver(IDynamicActorObserver observer)
            {
                this.observer = observer;
            }

            public void OnNext(Notification notification)
            {
                observer.OnNext(new DynamicNotification(notification.Source, notification.Message));
            }

            public bool Equals(DynamicActorObserver other)
            {
                return !ReferenceEquals(null, other)
                        && (ReferenceEquals(this, other)
                            || observer == other.observer);
            }

            public override bool Equals(object obj)
            {
                return !ReferenceEquals(null, obj)
                        && (ReferenceEquals(this, obj)
                            || obj.GetType() == typeof(DynamicActorObserver)
                                && Equals((DynamicActorObserver)obj));
            }

            public override int GetHashCode()
            {
                return observer.GetHashCode();
            }

            public static bool operator ==(DynamicActorObserver left, DynamicActorObserver right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(DynamicActorObserver left, DynamicActorObserver right)
            {
                return !Equals(left, right);
            }
        }

        internal static IActorProxy Proxy(ActorPath path)
        {
            return new DynamicActorProxy(Factory.Create(path), path);
        }

        class DynamicActorProxy : IActorProxy
        {
            readonly IDynamicActor actor;
            readonly ActorPath path;

            public DynamicActorProxy(IDynamicActor actor, ActorPath path)
            {
                this.actor = actor;
                this.path = path;
            }

            public Task OnTell(object message)
            {
                return actor.OnTell(new DynamicRequest(path, message));
            }

            public async Task<object> OnAsk(object message)
            {
                return (await actor.OnAsk(new DynamicRequest(path, message))).Message;
            }
        }

        static class Factory
        {
            public static IDynamicActor Create(ActorPath path)
            {
                var runtimeIdentity = ActorSystem.Dynamic.ActorType.Serializer(path);
                return DynamicActorFactory.GetGrain(runtimeIdentity);
            }
        }
    }
}