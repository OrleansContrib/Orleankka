using System;
using System.Threading.Tasks;

using Orleans;

namespace Orleankka
{
    using Core;
    using Meta;
    using Services;
    using Utility;

    public interface IActor
    {}

    public abstract class Actor : IActor
    {
        ActorRef self;

        protected Actor()
        {}

        protected Actor(string id, IActorRuntime runtime)
        {
            Requires.NotNull(runtime, nameof(runtime));
            Requires.NotNullOrWhitespace(id, nameof(id));

            Id = id;
            Runtime = runtime;
        }

        internal void Initialize(string id, IActorRuntime runtime, ActorPrototype prototype)
        {
            Id = id;
            Runtime = runtime;
            Prototype = prototype;
        }

        protected string Id
        {
            get; private set;
        }

        private IActorRuntime Runtime
        {
            get; set;
        }

        internal ActorPrototype Prototype
        {
            get; set;
        }

        protected IActorSystem System           => Runtime.System;
        protected IActivationService Activation => Runtime.Activation;
        protected IReminderService Reminders    => Runtime.Reminders;
        protected ITimerService Timers          => Runtime.Timers;

        protected ActorRef Self => self ?? (self = System.ActorOf(GetType(), Id));

        protected internal virtual void Define()
        {}

        protected internal virtual Task OnActivate()
        {
            return TaskDone.Done;
        }

        protected internal virtual Task OnDeactivate()
        {
            return TaskDone.Done;
        }

        protected internal virtual Task OnReminder(string id)
        {
            var message = $"Override {"OnReminder"}() method in class {GetType()} to implement corresponding behavior";

            throw new NotImplementedException(message);
        }

        protected internal virtual Task<object> OnReceive(object message)
        {
            return DispatchAsync(message);
        }

        protected void Dispatch(object message, Action<object> fallback = null)
        {
            Requires.NotNull(message, nameof(message));
            Prototype.Dispatch(this, message, fallback);
        }

        protected TResult DispatchResult<TResult>(object message, Func<object, object> fallback = null)
        {
            return (TResult)DispatchResult(message, fallback);
        }

        protected object DispatchResult(object message, Func<object, object> fallback = null)
        {
            Requires.NotNull(message, nameof(message));
            return Prototype.DispatchResult(this, message, fallback);
        }

        protected async Task<TResult> DispatchAsync<TResult>(object message, Func<object, Task<object>> fallback = null)
        {
            return (TResult)await DispatchAsync(message, fallback);
        }

        protected Task<object> DispatchAsync(object message, Func<object, Task<object>> fallback = null)
        {
            Requires.NotNull(message, nameof(message));
            return Prototype.DispatchAsync(this, message, fallback);
        }

        protected void On<TRequest, TResult>(Func<TRequest, TResult> handler)
        {
            Requires.NotNull(handler, nameof(handler));
            Prototype.RegisterHandler(handler.Method);
        }

        protected void On<TResult>(Func<Query<TResult>, TResult> handler)
        {
            Requires.NotNull(handler, nameof(handler));
            Prototype.RegisterHandler(handler.Method);
        }

        protected void On<TRequest, TResult>(Func<TRequest, Task<TResult>> handler)
        {
            Requires.NotNull(handler, nameof(handler));
            Prototype.RegisterHandler(handler.Method);
        }

        protected void On<TResult>(Func<Query<TResult>, Task<TResult>> handler)
        {
            Requires.NotNull(handler, nameof(handler));
            Prototype.RegisterHandler(handler.Method);
        }

        protected void On<TRequest>(Action<TRequest> handler)
        {
            Requires.NotNull(handler, nameof(handler));
            Prototype.RegisterHandler(handler.Method);
        }

        protected void On<TRequest>(Func<TRequest, Task> handler)
        {
            Requires.NotNull(handler, nameof(handler));
            Prototype.RegisterHandler(handler.Method);
        }
    }
}