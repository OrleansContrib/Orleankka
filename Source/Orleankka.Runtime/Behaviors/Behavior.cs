using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orleankka.Behaviors
{
    using Utility;

    public sealed class Behavior
    {
        Transition transition;

        readonly Dictionary<string, State> states;
        
        public Behavior(Dictionary<string, State> states = null) => 
            this.states = states ?? new Dictionary<string, State>();

        public State State(Receive behavior)
        {
            Requires.NotNull(behavior, nameof(behavior));
            return State(behavior.Method.Name) ;
        }

        public State State(string behavior)
        {
            var configured = states.Find(behavior);
            if (configured == null)
                throw new InvalidOperationException($"Missing configured behavior for '{behavior}'");

            return configured;
        }

        public void Initial(Receive behavior)
        {
            Requires.NotNull(behavior, nameof(behavior));
            var name = behavior.Method.Name;

            var configured = states.Find(name);
            Initial(configured ?? new State(name, behavior));
        }

        public void Initial(string behavior)
        {
            Requires.NotNull(behavior, nameof(behavior));

            Initial(State(behavior));
        }

        void Initial(State state)
        {
            if (Initialized())
                throw new InvalidOperationException($"Initial behavior has been already set to '{Current}'");

            Current = state;
        }

        public State Previous { get; private set; }
        public State Current  { get; private set; }

        bool Initialized() => Current != null;
        bool Switching => transition != null;

        public Task<object> Receive(object message)
        {
            if (!Initialized())
                throw new InvalidOperationException("Initial behavior should be set before receiving messages");

            return Current.Receive(message);
        }

        public Task Become(Receive behavior)
        {
            Requires.NotNull(behavior, nameof(behavior));
            var name = behavior.Method.Name;

            var configured = states.Find(name);
            return Become(configured ?? new State(name, behavior));
        }

        public Task Become(string behavior)
        {
            Requires.NotNull(behavior, nameof(behavior));

            return Become(State(behavior));
        }

        async Task Become(State next)
        {
            if (!Initialized())
                throw new InvalidOperationException("Initial behavior should be set before calling Become");

            if (Switching)
                throw new InvalidOperationException($"Can't become '{next}' while transition is already in progress: {transition}");

            if (Current.Name == next.Name)
                throw new InvalidOperationException($"Already behaving as '{next}'");

            transition = new Transition(@from: Current, to: next);

            await Current.HandleDeactivate(transition);
            await Current.HandleUnbecome(transition);

            Previous = Current;
            Current = next;

            await next.HandleBecome(transition);
            await next.HandleActivate(transition);

            transition = null;
        }
    }
}
