using System;
using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
        ActorRef self;
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

        public ActorRef Self
        {
            get { return (self ?? (self = ActorOf(new ActorPath(Interface.Of(GetType()), Id)))); }
        }

        public string Id
        {
            get { return (id ?? (id = IdentityOf(this))); }
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

        static string IdentityOf(Actor actor)
        {
            string id;
            actor.GetPrimaryKey(out id);
            return id;
        }        

        internal static bool IsCompatible(Type type)
        {
            return type.IsInterface && type != typeof(IActor) && typeof(IActor).IsAssignableFrom(type);
        }

        internal static IActorObserver Observer(ActorPath path)
        {
            return ActorObserverFactory.Cast(Factory.Create(path));
        }

        internal static IActorProxy Proxy(ActorPath path)
        {
            return new ActorProxy(Factory.Create(path));
        }

        class ActorProxy : IActorProxy
        {
            readonly IActor actor;

            public ActorProxy(IActor actor)
            {
                this.actor = actor;
            }

            public Task OnTell(object message)
            {
                return actor.OnTell(message);
            }

            public Task<object> OnAsk(object message)
            {
                return actor.OnAsk(message);
            }
        }

        static class Factory
        {
            static readonly ConcurrentDictionary<Type, Func<string, IActor>> cache =
                        new ConcurrentDictionary<Type, Func<string, IActor>>();

            public static IActor Create(ActorPath path)
            {
                var create = cache.GetOrAdd(path.Type, type =>
                {
                    var factory = type.Assembly
                        .ExportedTypes
                        .Where(IsOrleansCodegenedFactory)
                        .SingleOrDefault(x => x.GetMethod("Cast").ReturnType == type);

                    if (factory == null)
                        throw new ApplicationException("Can't find factory class for " + type);

                    return Bind(factory);
                });

                return create(path.Id);
            }

            static bool IsOrleansCodegenedFactory(Type type)
            {
                return type.GetCustomAttributes(typeof(GeneratedCodeAttribute), true)
                           .Cast<GeneratedCodeAttribute>()
                           .Any(x => x.Tool == "Orleans-CodeGenerator")
                       && type.Name.EndsWith("Factory");
            }

            static Func<string, IActor> Bind(IReflect factory)
            {
                var method = factory.GetMethod("GetGrain",
                    BindingFlags.Public | BindingFlags.Static, null,
                    new[] { typeof(string) }, null);

                var argument = Expression.Parameter(typeof(string), "primaryKey");
                var call = Expression.Call(method, new Expression[] { argument });
                var lambda = Expression.Lambda<Func<string, IActor>>(call, argument);

                return lambda.Compile();
            }
        }

        internal static class Interface
        {
            static readonly ConcurrentDictionary<Type, Type> cache =
                        new ConcurrentDictionary<Type, Type>();

            public static Type Of(Type type)
            {
                return cache.GetOrAdd(type, t =>
                {
                    var found = t.GetInterfaces()
                                 .Except(t.GetInterfaces().SelectMany(x => x.GetInterfaces()))
                                 .Where(x => typeof(IActor).IsAssignableFrom(x))
                                 .Where(x => x != typeof(IActor))
                                 .ToArray();

                    if (!found.Any())
                        throw new InvalidOperationException(
                            String.Format("The type '{0}' does not implement any of IActor inherited interfaces", t));

                    return found[0];
                });
            }
        }
    }
}
