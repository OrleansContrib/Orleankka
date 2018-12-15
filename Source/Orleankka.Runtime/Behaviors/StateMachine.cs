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

        public StateMachine State(string name, Receive behavior, Func<Receive, Receive> extend = null)
        {
            Requires.NotNull(name, nameof(name));
            Requires.NotNull(behavior, nameof(behavior));

            return Add(name, behavior, null, extend);
        }

        public StateMachine State(string name, Receive behavior, string super, Func<Receive, Receive> extend = null)
        {
            Requires.NotNull(name, nameof(name));
            Requires.NotNull(behavior, nameof(behavior));
            Requires.NotNull(super, nameof(super));

            return Add(name, behavior, super, extend);
        }

        public StateMachine State(Receive behavior, Func<Receive, Receive> extend = null)
        {
            Requires.NotNull(behavior, nameof(behavior));

            return Add(behavior.Method.Name, behavior, null, extend);
        }

        public StateMachine State(Receive behavior, Receive super, Func<Receive, Receive> extend = null)
        {
            Requires.NotNull(behavior, nameof(behavior));
            Requires.NotNull(super, nameof(super));

            return Add(behavior.Method.Name, behavior, super.Method.Name, extend);
        }

        StateMachine Add(string name, Receive behavior, string super, Func<Receive, Receive> extend)
        {
            if (configuration.ContainsKey(name))
                throw new InvalidOperationException($"State '{name}' has been already configured");

            if (extend != null)
                behavior = extend(behavior);

            configuration[name] = new StateConfiguration(name, behavior, super);
            return this;
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