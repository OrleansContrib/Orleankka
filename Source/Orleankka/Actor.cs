using System;
using System.Linq;
using System.Threading.Tasks;

using Orleans;

namespace Orleankka
{
    using Core;
    using Utility;

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

        internal void Initialize(string id, IActorSystem system, ActorEndpoint endpoint, ActorPrototype prototype)
        {
            Id = id;
            System = system;
            Endpoint = endpoint;
            Prototype = prototype;
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

        internal ActorPrototype Prototype
        {
            get; set;
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
            return Prototype.Dispatch(this, message);
        }

        public virtual Task OnReminder(string id)
        {
            var message = string.Format("Override {0}() method in class {1} to implement corresponding behavior", 
                                        "OnReminder", GetType());

            throw new NotImplementedException(message);
        }

        protected internal virtual void Define()
        {}

        protected void Reentrant(Func<object, bool> predicate)
        {
            Requires.NotNull(predicate, "predicate");
            Prototype.RegisterReentrant(predicate);
        }
    }
}