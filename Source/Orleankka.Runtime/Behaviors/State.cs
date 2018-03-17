using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Orleankka.Behaviors
{
    using Utility;

    [DebuggerDisplay("{ToDebugString()}")]
    public class State
    {
        public string Name { get; }
        public Receive Behavior { get; }
        public State Super { get; }

        internal State(string name, Receive behavior, State super = null)
        {
            Requires.NotNull(name, nameof(name));
            Requires.NotNull(behavior, nameof(behavior));

            Name = name;
            Behavior = behavior;
            Super = super;
        }

        internal async Task<object> Receive(object message)
        {
            switch (message)
            {
                case BehaviorMessage _:
                    throw new InvalidOperationException("Use specialized Become/Unbecome functions to transition to a different state");
                
                case Activate _:
                    await HandleActivate(default(Transition));
                    return Done.Result;
                
                case Deactivate _:
                    await HandleDeactivate(default(Transition));
                    return Done.Result;
            }

            return await Behavior(message);
        }

        internal async Task HandleBecome(Transition transition)
        {
            if (Includes(transition.From))
                return;

            if (Super != null)
                await Super.HandleBecome(transition);

            await Behavior(Become.Message);
        }

        internal async Task HandleUnbecome(Transition transition)
        {
            if (Includes(transition.To))
                return;

            await Behavior(Unbecome.Message);

            if (Super != null)
                await Super.HandleUnbecome(transition);
        }

        internal async Task HandleActivate(Transition transition)
        {
            if (Includes(transition.From))
                return;

            if (Super != null)
                await Super.HandleActivate(transition);

            await Behavior(Activate.Message); 
        }

        internal async Task HandleDeactivate(Transition transition)
        {
            if (Includes(transition.To))
                return;

            await Behavior(Deactivate.Message);

            if (Super != null)
                await Super.HandleDeactivate(transition);
        }
        
        bool Includes(State state) => state.RootedTo(this);
        bool RootedTo(State state) => Super == state || (Super?.RootedTo(state) ?? false);

        string ToDebugString() => Super != null 
            ? $"[{Name}]->{Super.ToDebugString()}"
            : $"[{Name}]";

        public override string ToString() => Name;
    }
}