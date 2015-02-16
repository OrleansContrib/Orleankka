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

        NotImplementedException NotImplemented(string method)
        {
            return new NotImplementedException(String.Format(
                "Override {0}() method in class {1} to implement corresponding behavior", 
                method, GetType())
            );
        }

        protected static Task Done()
        {
            return TaskDone.Done;
        }

        protected static Task<object> Result<T>(T arg)
        {
            return Task.FromResult((object)arg);
        }

        [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
        public class ConfigurationAttribute : ActorConfigurationAttribute
        {
            public ConfigurationAttribute(
                Placement placement = Placement.Random,
                Concurrency concurrency = Concurrency.Sequential,
                Delivery delivery = Delivery.Ordered)
            {
                Configuration = ActorConfiguration.Actor(placement, concurrency, delivery);
            }
        }
    }
}