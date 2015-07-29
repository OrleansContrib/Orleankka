using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Orleans;

namespace Orleankka
{
    using Meta;
    using Utility;
    using Services;
    
    public abstract class Actor
    {
        ActorRef self;

        protected Actor()
        {}

        protected Actor(string id, IActorRuntime runtime)
        {
            Requires.NotNull(runtime, "runtime");
            Requires.NotNullOrWhitespace(id, "id");

            Id = id;
            Runtime = runtime;
        }

        internal void Initialize(string id, IActorRuntime runtime, ActorPrototype prototype)
        {
            Id = id;
            Runtime = runtime;
            _ = prototype;
        }

        protected string Id
        {
            get; private set;
        }

        private IActorRuntime Runtime
        {
            get; set;
        }

        protected IActorSystem System
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return Runtime.System; }
        }

        protected IActivationService Activation
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return Runtime.Activation; }
        }

        protected IReminderService Reminders
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return Runtime.Reminders; }
        }

        protected ITimerService Timers
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return Runtime.Timers; }
        }

        protected internal ActorPrototype _
        {
            get; set;
        }

        protected internal virtual Type Prototype
        {
            get { return typeof(ActorPrototype); }
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
            var message = string.Format("Override {0}() method in class {1} to implement corresponding behavior", 
                                        "OnReminder", GetType());

            throw new NotImplementedException(message);
        }

        protected internal virtual Task<object> OnReceive(object message)
        {
            return DispatchAsync(message);
        }

        protected void Reentrant(Func<object, bool> evaluator)
        {
            Requires.NotNull(evaluator, "evaluator");
            _.RegisterReentrant(evaluator);
        }

        protected void Dispatch(object message)
        {
            Requires.NotNull(message, "message");
            _.Dispatch(this, message);
        }

        protected TResult DispatchResult<TResult>(object message)
        {
            return (TResult)DispatchResult(message);
        }

        protected object DispatchResult(object message)
        {
            Requires.NotNull(message, "message");
            return _.DispatchResult(this, message);
        }

        protected async Task<TResult> DispatchAsync<TResult>(object message)
        {
            return (TResult)await DispatchAsync(message);
        }

        protected Task<object> DispatchAsync(object message)
        {
            Requires.NotNull(message, "message");
            return _.DispatchAsync(this, message);
        }

        protected void On<TRequest, TResult>(Func<TRequest, TResult> handler)
        {
            Requires.NotNull(handler, "handler");
            _.RegisterHandler(handler.Method);
        }

        protected void On<TResult>(Func<Query<TResult>, TResult> handler)
        {
            Requires.NotNull(handler, "handler");
            _.RegisterHandler(handler.Method);
        }

        protected void On<TRequest, TResult>(Func<TRequest, Task<TResult>> handler)
        {
            Requires.NotNull(handler, "handler");
            _.RegisterHandler(handler.Method);
        }

        protected void On<TResult>(Func<Query<TResult>, Task<TResult>> handler)
        {
            Requires.NotNull(handler, "handler");
            _.RegisterHandler(handler.Method);
        }

        protected void On<TRequest>(Action<TRequest> handler)
        {
            Requires.NotNull(handler, "handler");
            _.RegisterHandler(handler.Method);
        }

        protected void On<TRequest>(Func<TRequest, Task> handler)
        {
            Requires.NotNull(handler, "handler");
            _.RegisterHandler(handler.Method);
        }

        protected void KeepAlive(TimeSpan timeout)
        {
            _.SetKeepAlive(timeout);
        }
    }

    public abstract class Actor<TPrototype> : Actor where TPrototype : ActorPrototype
    {
        protected Actor()
        {}

        protected Actor(string id, IActorRuntime runtime)
            : base(id, runtime)
        {}

        protected internal override Type Prototype
        {
            get { return typeof(TPrototype); }
        }

        protected new TPrototype _
        {
            get { return (TPrototype) base._; }
        }
    }
}