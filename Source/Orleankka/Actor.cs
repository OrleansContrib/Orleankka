using System;
using System.Linq;
using System.Threading.Tasks;

using Orleans;

namespace Orleankka
{
    using Core;
    using Meta;
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

        protected string Id
        {
            get; private set;
        }

        protected IActorSystem System
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

        protected ActorRef Self
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

        protected internal virtual Task OnActivate()
        {
            return TaskDone.Done;
        }

        protected internal virtual Task OnDeactivate()
        {
            return TaskDone.Done;
        }

        protected internal virtual Task<object> OnReceive(object message)
        {
            return DispatchAsync(message);
        }

        protected internal virtual Task OnReminder(string id)
        {
            var message = string.Format("Override {0}() method in class {1} to implement corresponding behavior", 
                                        "OnReminder", GetType());

            throw new NotImplementedException(message);
        }

        protected internal virtual void Define()
        {}

        protected void Reentrant(Func<object, bool> evaluator)
        {
            Requires.NotNull(evaluator, "evaluator");
            Prototype.RegisterReentrant(evaluator);
        }

        protected void Reentrant<T>(Func<T, bool> evaluator)
        {
            Requires.NotNull(evaluator, "evaluator");
            Prototype.RegisterReentrant(x => evaluator((T)x));
        }

        protected void Dispatch(object message)
        {
            Requires.NotNull(message, "message");
            Prototype.Dispatch(this, message);
        }

        protected TResult DispatchResult<TResult>(object message)
        {
            return (TResult) DispatchResult(message);
        }

        protected object DispatchResult(object message)
        {
            Requires.NotNull(message, "message");
            return Prototype.DispatchResult(this, message);
        }

        protected async Task<TResult> DispatchAsync<TResult>(object message)
        {
            return (TResult) await DispatchAsync(message);
        }

        protected Task<object> DispatchAsync(object message)
        {
            Requires.NotNull(message, "message");
            return Prototype.DispatchAsync(this, message);
        }

        protected void On<TRequest, TResult>(Func<TRequest, TResult> handler)
        {
            Requires.NotNull(handler, "handler");
            Prototype.RegisterHandler(handler.Method);
        }

        protected void On<TResult>(Func<Query<TResult>, TResult> handler)
        {
            Requires.NotNull(handler, "handler");
            Prototype.RegisterHandler(handler.Method);
        }

        protected void On<TRequest, TResult>(Func<TRequest, Task<TResult>> handler)
        {
            Requires.NotNull(handler, "handler");
            Prototype.RegisterHandler(handler.Method);
        }

        protected void On<TResult>(Func<Query<TResult>, Task<TResult>> handler)
        {
            Requires.NotNull(handler, "handler");
            Prototype.RegisterHandler(handler.Method);
        }

        protected void On<TRequest>(Action<TRequest> handler)
        {
            Requires.NotNull(handler, "handler");
            Prototype.RegisterHandler(handler.Method);
        }

        protected void On<TRequest>(Func<TRequest, Task> handler)
        {
            Requires.NotNull(handler, "handler");
            Prototype.RegisterHandler(handler.Method);
        }

        protected void KeepAlive(TimeSpan timeout)
        {
            Prototype.SetKeepAlive(timeout);
        }
    }
}