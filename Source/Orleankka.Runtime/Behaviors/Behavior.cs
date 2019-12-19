using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Orleankka.Behaviors
{
    using Utility;

    [DebuggerDisplay("{ToDebugString()}")]
    public sealed class Behavior
    {
        Transition transition;

        readonly Stack<State> history = new Stack<State>();
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

        public string Etag { get; private set; }
        public State Previous { get; private set; }

        State current;
        public State Current
        {
            get => current;
            private set
            {
                current = value;
                Etag = Guid.NewGuid().ToString("N");
            }
        }

        bool Initialized() => Current != null;
        bool Switching => transition != null;

        public Task<object> Receive(object message)
        {
            if (!Initialized())
                throw new InvalidOperationException("Initial behavior should be set before receiving messages");

            return Current.Receive(message);
        }

        public Task Become(Receive behavior) => HandleBecome(behavior, Behaviors.Become.Message);
        public Task Become<TArg>(Receive behavior, TArg arg) => HandleBecome(behavior, new Become<TArg>(arg));

        Task HandleBecome(Receive behavior, Become message)
        {
            Requires.NotNull(behavior, nameof(behavior));
            
            var state = states.Find(behavior.Method.Name) 
                        ?? new State(behavior.Method.Name, behavior);

            return HandleBecome(state, message);
        }

        public Task Become(string behavior) => HandleBecome(behavior, Behaviors.Become.Message);
        public Task Become<TArg>(string behavior, TArg arg) => HandleBecome(behavior, new Become<TArg>(arg));

        Task HandleBecome(string behavior, Become message)
        {
            Requires.NotNull(behavior, nameof(behavior));

            return HandleBecome(State(behavior), message);
        }

        public Task BecomeStacked(Receive behavior) => HandleBecomeStacked(behavior, Behaviors.Become.Message);
        public Task BecomeStacked<TArg>(Receive behavior, TArg arg) => HandleBecomeStacked(behavior, new Become<TArg>(arg));

        Task HandleBecomeStacked(Receive behavior, Become message)
        {
            Requires.NotNull(behavior, nameof(behavior));
            
            var state = states.Find(behavior.Method.Name) 
                        ?? new State(behavior.Method.Name, behavior);

            return HandleBecomeStacked(state, message);
        }

        public Task BecomeStacked(string behavior) => HandleBecomeStacked(behavior, Behaviors.Become.Message);
        public Task BecomeStacked<TArg>(string behavior, TArg arg) => HandleBecomeStacked(behavior, new Become<TArg>(arg));

        Task HandleBecomeStacked(string behavior, Become message)
        {
            Requires.NotNull(behavior, nameof(behavior));

            return HandleBecomeStacked(State(behavior), message);
        }

        async Task HandleBecomeStacked(State next, Become message)
        {
            history.Push(Current);

            await HandleBecome(next, message);
        }

        public async Task Unbecome()
        {
            if (history.Count == 0)
                throw new InvalidOperationException("The previous behavior has not been recorded. Use BecomeStacked method to stack behaviors");

            await HandleBecome(history.Pop(), Behaviors.Become.Message);
        }

        async Task HandleBecome(State next, Become message)
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

            await next.HandleBecome(transition, message);
            await next.HandleActivate(transition);

            transition = null;
        }

        string ToDebugString() => Current != null 
            ? $"{Current.ToDebugString()} ({states.Count} states)"
            : $"({states.Count} states)";
    }
}
