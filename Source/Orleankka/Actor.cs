using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Orleans;

namespace Orleankka
{
    using Core;

    public abstract class Actor
    {
        ActorRef self;

        protected Actor()
        {}

        protected Actor(string id, IActorSystem system)
        {
            Requires.NotNull(system, "system");
            Requires.NotNullOrWhitespace(id, "id");

            Id = id;
            System = system;
        }

        internal void Initialize(string id, IActorSystem system, ActorEndpoint endpoint)
        {
            Id = id;
            System = system;
            Endpoint = endpoint;
        }

        public string Id
        {
            get; private set;
        }

        public IActorSystem System
        {
            get; private set;
        }

        internal ActorEndpoint Endpoint
        {
            get; private set;
        }

        public ActorRef Self
        {
            get
            {
                if (self == null)
                {
                    var path = ActorPath.From(GetType(), Id);
                    self = System.ActorOf(path);
                }

                return self;
            }
        }

        public virtual Task OnActivate()
        {
            return TaskDone.Done;
        }

        public virtual Task<object> OnReceive(object message)
        {
            throw NotImplemented("OnReceive");
        }

        public virtual Task OnReminder(string id)
        {
            throw NotImplemented("OnReminder");
        }

        NotImplementedException NotImplemented(string method)
        {
            return new NotImplementedException(String.Format(
                "Override {0}() method in class {1} to implement corresponding behavior", 
                method, GetType())
            );
        }

        protected static Task<object> Done()
        {
            return CompletedTask;
        }

        protected static Task<object> Result<T>(T arg)
        {
            return Task.FromResult((object)arg);
        }

        static readonly Task<object> CompletedTask = Task.FromResult((object)null);
        
        class Message
        {
            static readonly Dictionary<Type, Type> interleaved =
                        new Dictionary<Type, Type>();

            internal static void Register(MemberInfo actor)
            {
            }

            internal static void Reset()
            {
                interleaved.Clear();
            }

            internal static bool Interleaved(Type message)
            {
                return interleaved.ContainsKey(message);
            }

        }
    }

    class ActorDefinition
    {
        static readonly Dictionary<Type, ActorDefinition> cache =
                    new Dictionary<Type, ActorDefinition>();

        readonly HashSet<Type> interleave;

        internal static void Register(Type actor)
        {
            var definition = new ActorDefinition(actor);
            cache.Add(actor, definition);
        }

        internal static void Reset()
        {
            cache.Clear();
        }

        internal static ActorDefinition Of(Type actor)
        {
            ActorDefinition definition = cache.Find(actor);
            return definition ?? new ActorDefinition(actor);
        }

        ActorDefinition(Type actor)
        {
            var attributes = actor.GetCustomAttributes<InterleaveAttribute>(inherit: true);
            interleave = new HashSet<Type>(attributes.Select(x => x.Message));
        }

        internal bool Interleaved(Type message)
        {
            return interleave.Contains(message);
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ActorAttribute : Attribute
    {
        public Placement Placement {get; set;}
    }
}