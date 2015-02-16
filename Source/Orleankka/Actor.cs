using System;
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

        internal void Initialize(string id, IActorSystem system, ActorEndpointBase endpoint)
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

        internal ActorEndpointBase Endpoint
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
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ActorAttribute : ActorConfigurationAttribute
    {
        public ActorAttribute(Placement placement = Placement.Auto, Delivery delivery = Delivery.Ordered)
        {
            Configuration = ActorConfiguration.Actor(placement, delivery);
        }
    }
}