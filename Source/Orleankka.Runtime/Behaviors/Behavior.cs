using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orleankka.Behaviors
{
    using Utility;

    public sealed class Behavior
    {
        State current;
        Transition transition;

        readonly Dictionary<string, State> states;
        
        public Behavior(Dictionary<string, State> states = null)
        {
            this.states = states ?? new Dictionary<string, State>();
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

            var configured = states.Find(behavior);
            if (configured == null)
                throw new InvalidOperationException($"Missing configured behavior for '{behavior}'");

            Initial(configured);
        }

        void Initial(State state)
        {
            if (Initialized())
                throw new InvalidOperationException($"Initial behavior has been already set to '{Current}'");

            current = state;
        }

        bool Switching => transition != null;
        bool Initialized() => Current != null;
        public string Current => current?.Name;

        public Task<object> Receive(object message)
        {
            if (!Initialized())
                throw new InvalidOperationException("Initial behavior should be set before receiving messages");

            return current.Receive(message);
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

            var configured = states.Find(behavior);
            if (configured == null)
                throw new InvalidOperationException($"Missing configured behavior for '{behavior}'");

            return Become(configured);
        }

        async Task Become(State next)
        {
            if (!Initialized())
                throw new InvalidOperationException("Initial behavior should be set before calling Become");

            if (Switching)
                throw new InvalidOperationException($"Can't become '{next}' while transition is already in progress: {transition}");

            if (current.Name == next.Name)
                throw new InvalidOperationException($"Already behaving as '{next}'");

            transition = new Transition(@from: current, to: next);

            await current.HandleDeactivate(transition);
            await current.HandleUnbecome(transition);

            current = next;

            await next.HandleBecome(transition);
            await next.HandleActivate(transition);

            transition = null;
        }
    }
}