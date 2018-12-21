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

        public StateMachine State(string name, Receive behavior, Func<Receive, Receive> extend = null)
        {
            return State(name, behavior, null, Array.Empty<Receive>(), extend);
        }

        public StateMachine State(string name, Receive behavior, Receive[] trait, Func<Receive, Receive> extend = null)
        {
            Requires.NotNull(name, nameof(name));
            Requires.NotNull(behavior, nameof(behavior));
            Requires.NotNull(trait, nameof(trait));

            last = Add(name, behavior, null, extend, trait);
            return this;
        }

        public StateMachine State(string name, Receive behavior, string super, Func<Receive, Receive> extend = null)
        {
            return State(name, behavior, super, Array.Empty<Receive>(), extend);
        }

        public StateMachine State(string name, Receive behavior, string super, Receive[] trait, Func<Receive, Receive> extend = null)
        {
            Requires.NotNull(name, nameof(name));
            Requires.NotNull(behavior, nameof(behavior));
            Requires.NotNull(super, nameof(super));
            Requires.NotNull(trait, nameof(trait));

            last = Add(name, behavior, super, extend, trait);
            return this;
        }

        public StateMachine State(Receive behavior, Func<Receive, Receive> extend = null)
        {
            return State(behavior, behavior, Array.Empty<Receive>(), extend);
        }

        public StateMachine State(Receive behavior, Receive[] trait, Func<Receive, Receive> extend = null)
        {
            Requires.NotNull(behavior, nameof(behavior));
            Requires.NotNull(trait, nameof(trait));

            last = Add(behavior.Method.Name, behavior, null, extend, trait);
            return this;
        }

        public StateMachine State(Receive behavior, Receive super, Func<Receive, Receive> extend = null)
        {
            return State(behavior, super, Array.Empty<Receive>(), extend);
        }

        public StateMachine State(Receive behavior, Receive super, Receive[] trait, Func<Receive, Receive> extend = null)
        {
            Requires.NotNull(behavior, nameof(behavior));
            Requires.NotNull(super, nameof(super));
            Requires.NotNull(trait, nameof(trait));

            last = Add(behavior.Method.Name, behavior, super.Method.Name, extend, trait);
            return this;
        }

        public StateMachine Substate(string name, Receive behavior, Func<Receive, Receive> extend = null)
        {
            return Substate(name, behavior, Array.Empty<Receive>(), extend);
        }

        public StateMachine Substate(string name, Receive behavior, Receive[] trait, Func<Receive, Receive> extend = null)
        {
            Requires.NotNull(trait, nameof(trait));
            return State(name, behavior, last.Name, trait, extend);
        }
        
        public StateMachine Substate(Receive behavior, Func<Receive, Receive> extend = null)
        {
            return Substate(behavior, Array.Empty<Receive>(), extend);
        }

        public StateMachine Substate(Receive behavior, Receive[] trait, Func<Receive, Receive> extend = null)
        {
            Requires.NotNull(trait, nameof(trait));

            if (last == null)
                throw new InvalidOperationException("No previous state were specified");

            return State(behavior?.Method.Name, behavior, last.Name, trait, extend);
        }

        StateConfiguration Add(string name, Receive behavior, string super, Func<Receive, Receive> extend, Receive[] trait)
        {
            if (configuration.ContainsKey(name))
                throw new InvalidOperationException($"State '{name}' has been already configured");

            if (extend != null)
                behavior = extend(behavior);

            var state = new StateConfiguration(name, behavior.Join(trait), super);
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