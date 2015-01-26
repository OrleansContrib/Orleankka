using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

using Orleans;
using Orleans.Runtime;

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
                return DynamicActorFactory.GetGrain(path.ToString());
            }
        }
    }

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

    partial class ActorSystem
    {
        /// <summary>
        /// Global configuration options for dynamic actor feature.
        /// </summary>
        public static class Dynamic
        {
            static Dynamic()
            {
                Activator = path => (DynamicActor) System.Activator.CreateInstance(path.Type);

                Serializer = obj =>
                {
                    using (var ms = new MemoryStream())
                    {
                        new BinaryFormatter().Serialize(ms, obj);
                        return ms.ToArray();
                    }
                };

                Deserializer = bytes =>
                {
                    using (var ms = new MemoryStream(bytes))
                    {
                        var formatter = new BinaryFormatter();
                        return formatter.Deserialize(ms);
                    }
                };
            }

            /// <summary>
            /// The activation function, which creates actual instances of <see cref="Orleankka.DynamicActor"/>
            /// </summary>
            /// <remarks>
            /// By default expects type to have a public parameterless constructor 
            /// as a consequence of using standard  <see cref="System.Activator"/>
            /// </remarks>
            public static Func<ActorPath, DynamicActor> Activator { get; set; }

            /// <summary>
            /// The serialization function, which serializes messages to byte[]
            /// </summary>
            /// <remarks>
            /// By default uses standard binary serialization provided by <see cref="BinaryFormatter"/>
            /// </remarks>
            public static Func<object, byte[]> Serializer
            {
                get { return DynamicMessage.Serializer; }
                set { DynamicMessage.Serializer = value; }
            }

            /// <summary>
            /// The deserialization function, which deserializes byte[] back to messages
            /// </summary>
            /// <remarks>
            /// By default uses standard binary serialization provided by 
            /// <see cref="BinaryFormatter"/></remarks>
            public static Func<byte[], object> Deserializer
            {
                get { return DynamicMessage.Deserializer; }
                set { DynamicMessage.Deserializer = value; }
            }
        }
    }
}