using System;
using System.Collections.Generic;

namespace Orleankka.Behaviors
{
    using Utility;

    public class StateMachine
    {
        readonly Dictionary<string, StateConfiguration> configuration = 
             new Dictionary<string, StateConfiguration>();

        public Dictionary<string, State> Build()
        {
            var result = new Dictionary<string, State>();

            foreach (var each in configuration.Values)
                Build(each, result);

            return result;
        }

        public static implicit operator Dictionary<string, State>(StateMachine x) => x.Build();

        void Build(StateConfiguration state, IDictionary<string, State> states)
        {
            if (states.ContainsKey(state.Name))
                return;

            if (state.Super == null)
            {
                states.Add(state.Name, new State(state.Name, state.Behavior));
                return;
            }

            if (!configuration.TryGetValue(state.Super, out var super))
                throw new InvalidOperationException($"Super '{state.Super}' specified for state '{state.Name}' hasn't been configured");

            // recurse
            Build(super, states);
            
            // now get fully configured super
            states.Add(state.Name, new State(state.Name, state.Behavior, states[state.Super]));
        }

        public StateMachine State(string name, Receive behavior, string super = null)
        {
            Requires.NotNull(name, nameof(name));
            Requires.NotNull(behavior, nameof(behavior));

            if (configuration.ContainsKey(name))
                throw new InvalidOperationException($"State '{name}' has been already configured");

            configuration[name] = new StateConfiguration(name, behavior, super);
            return this;
        }

        public StateMachine State(Receive behavior, Receive super = null)
        {
            Requires.NotNull(behavior, nameof(behavior));
            
            return State(behavior.Method.Name, behavior, super?.Method.Name);
        }

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
    }
}