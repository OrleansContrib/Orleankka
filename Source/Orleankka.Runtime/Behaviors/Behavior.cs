using System;
using System.Threading.Tasks;

namespace Orleankka.Behaviors
{
    using Utility;

    public sealed class Behavior
    {
        readonly ActorGrain actor;

        public Behavior(ActorGrain actor)
        {
            Requires.NotNull(actor, nameof(actor));
            this.actor = actor;
        }

        public Behavior(ActorGrain actor, Receive initial)
        {
            Requires.NotNull(actor, nameof(initial));
            Requires.NotNull(initial, nameof(initial));

            this.actor = actor;
            Initial(initial);
        }

        public Func<Transition, Task> OnTransitioning { get; set; } = t => Task.CompletedTask;
        public Func<Transition, Task> OnTransitioned  { get; set; } = t => Task.CompletedTask;
        public Func<Transition, Exception, Task> OnTransitionError { get; set; } = (t, e) => Task.CompletedTask;

        public Task<object> OnReceive(object message)
        {
            if (!Initialized())
                throw new InvalidOperationException($"Initial behavior should be set for actor '{actor}' in order to receive messages");

            return Current(message);
        }

        public void Initial(Receive behavior)
        {
            Requires.NotNull(behavior, nameof(behavior));

            if (Initialized())
                throw new InvalidOperationException($"Initial behavior has been already set to '{CurrentName}'");

            Current = behavior;
        }

        bool Initialized() => Current != null;

        public Receive Current { get; private set; }
        public string CurrentName => Current?.Method.Name;

        public async Task Become(Receive behavior)
        {
            Requires.NotNull(behavior, nameof(behavior));

            if (!Initialized())
                throw new InvalidOperationException($"Initial behavior should be set for actor '{actor}' before calling Become");

            if (CurrentName == behavior.Method.Name)
                throw new InvalidOperationException($"Actor '{actor}' is already behaving as '{behavior}'");

            var transition = new Transition(Current, behavior);

            try
            {
                await OnTransitioning(transition);

                await Current(ActorGrain.Deactivate.Message);
                await Current(ActorGrain.Unbecome.Message);

                await behavior(ActorGrain.Become.Message);
                await behavior(ActorGrain.Activate.Message);

                Current = behavior;
                await OnTransitioned(transition);
            }
            catch (Exception exception)
            {
                await OnTransitionError(transition, exception);
            }
        }
    }
}