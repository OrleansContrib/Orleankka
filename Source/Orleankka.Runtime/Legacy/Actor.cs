using System;
using System.Threading.Tasks;

using Orleans.Runtime;

namespace Orleankka.Legacy
{
    using Services;
    using Utility;
    using Behaviors;

    public abstract class Actor : DispatchActorGrain
    {
        public ActorBehavior Behavior   { get; }

        protected Actor()
        {
            Behavior = ActorBehavior.Null(this);
        }

        /// <summary>
        /// Provided only for unit-testing purposes
        /// </summary>
        protected Actor(Dispatcher dispatcher = null)
            : this(null, null, dispatcher)
        {}

        /// <summary>
        /// Provided only for unit-testing purposes
        /// </summary>
        protected Actor(IActorRuntime runtime = null, Dispatcher dispatcher = null) 
            : this(null, runtime, dispatcher)
        {}

        /// <summary>
        /// Provided only for unit-testing purposes
        /// </summary>
        protected Actor(string id = null, Dispatcher dispatcher = null) 
            : this(id, null, dispatcher)
        {}

        /// <summary>
        /// Provided only for unit-testing purposes
        /// </summary>
        protected Actor(string id = null, IActorRuntime runtime = null, Dispatcher dispatcher = null)
            : base(id, runtime, dispatcher)
        {
            Behavior = ActorBehavior.Null(this);
        }

        public override async Task<object> Receive(object message)
        {
            switch (message)
            {
                case Activate _ : 
                    await OnActivate(); 
                    return Done;
                case Deactivate _ : 
                    await OnDeactivate(); 
                    return Done;
                default: return await OnReceive(message);
            }
        }

        public virtual Task OnActivate() => Behavior.HandleActivate();
        public virtual Task OnDeactivate() => Behavior.HandleDeactivate();

        public virtual Task<object> OnReceive(object message)
        {
            RequestContext.Set(TimerService.RequestContextId, null);
            return Behavior.HandleReceive(message);
        }

        public virtual Task OnReminder(string id) => Behavior.HandleReminder(id);

        public async Task<TResult> Dispatch<TResult>(object message, Func<object, Task<object>> fallback = null) => 
            (TResult)await Dispatch(message, fallback);

        public Task<object> Dispatch(object message, Func<object, Task<object>> fallback = null)
        {
            Requires.NotNull(message, nameof(message));
            return Dispatcher.Dispatch(this, message, fallback);
        }

        public virtual Task<object> OnUnhandledReceive(RequestOrigin origin, object message) =>
            throw new UnhandledMessageException(this, message, $" in its current behavior '{Behavior.Current}'");

        public virtual Task OnUnhandledReminder(string id) =>
            throw new UnhandledReminderException(this, id);

        public virtual Task OnTransitioning(Transition transition) => Task.CompletedTask;
        public virtual Task OnTransitioned(Transition transition) => Task.CompletedTask;
        public virtual Task OnTransitionFailure(Transition transition, Exception exception) => Task.CompletedTask;
    }
}