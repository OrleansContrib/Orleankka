using System;
using System.Collections.Generic;

namespace Orleankka.Behaviors
{
    using Utility;

    public class StateMachine
    {
        class StateConfiguration
        {
            public readonly string Name;
            public readonly Receive Behavior;
            public readonly string Super;

            public StateConfiguration(string name, Receive behavior, string super)
            {
                Name = name;
                Behavior = behavior;
                Super = super;
            }
        }

        readonly Dictionary<string, StateConfiguration> configuration = 
             new Dictionary<string, StateConfiguration>();

        StateConfiguration last;

        public StateMachine State(string name, Receive behavior, Func<Receive, Receive> extend = null, params Receive[] trait)
        {
            Requires.NotNull(name, nameof(name));
            Requires.NotNull(behavior, nameof(behavior));

            last = Add(name, behavior, null, extend, trait);
            return this;
        }

        public StateMachine State(string name, Receive behavior, string super, Func<Receive, Receive> extend = null, params Receive[] trait)
        {
            Requires.NotNull(name, nameof(name));
            Requires.NotNull(behavior, nameof(behavior));
            Requires.NotNull(super, nameof(super));

            last = Add(name, behavior, super, extend, trait);
            return this;
        }

        public StateMachine State(Receive behavior, Func<Receive, Receive> extend = null, params Receive[] trait)
        {
            Requires.NotNull(behavior, nameof(behavior));

            last = Add(behavior.Method.Name, behavior, null, extend, trait);
            return this;
        }

        public StateMachine State(Receive behavior, Receive super, Func<Receive, Receive> extend = null, params Receive[] trait)
        {
            Requires.NotNull(behavior, nameof(behavior));
            Requires.NotNull(super, nameof(super));

            last = Add(behavior.Method.Name, behavior, super.Method.Name, extend, trait);
            return this;
        }

        public StateMachine Substate(string name, Receive behavior, Func<Receive, Receive> extend = null, params Receive[] trait)
        {
            if (last == null)
                throw new InvalidOperationException("No previous state were specified");

            return State(name, behavior, last.Name, extend, trait);
        }

        public StateMachine Substate(Receive behavior, Func<Receive, Receive> extend = null, params Receive[] trait)
        {
            if (last == null)
                throw new InvalidOperationException("No previous state were specified");

            return State(behavior?.Method.Name, behavior, last.Name, extend, trait);
        }

        StateConfiguration Add(string name, Receive behavior, string super, Func<Receive, Receive> extend, Receive[] trait)
        {
            if (configuration.ContainsKey(name))
                throw new InvalidOperationException($"State '{name}' has been already configured");

            if (extend != null)
                behavior = extend(behavior);

            var state = new StateConfiguration(name, behavior.Trait(trait), super);
            configuration[name] = state;

            return state;
        }

        public Dictionary<string, State> Build()
        {
            var result = new Dictionary<string, State>();

            foreach (var each in configuration.Values)
                Build(each, result, new List<string>());

            return result;
        }

        void Build(StateConfiguration state, IDictionary<string, State> states, ICollection<string> chain)
        {
            if (chain.Contains(state.Name))
                throw new InvalidOperationException("Cycle detected: " + string.Join(" -> ", chain) + $" !-> {state.Name}");

            if (states.ContainsKey(state.Name))
                return;

            if (state.Super == null)
            {
                states.Add(state.Name, new State(state.Name, state.Behavior));
                return;
            }

            if (!configuration.TryGetValue(state.Super, out var super))
                throw new InvalidOperationException($"Super '{state.Super}' specified for state '{state.Name}' hasn't been configured");

            chain.Add(state.Name);

            // recurse
            Build(super, states, chain);

            // now get fully configured super
            states.Add(state.Name, new State(state.Name, state.Behavior, states[state.Super]));
        }

        public static implicit operator Dictionary<string, State>(StateMachine x) => x.Build();
    }
}