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
                    await HandleActivate(Transition.Initial);
                    return Done.Result;
                
                case Deactivate _:
                    await HandleDeactivate(Transition.Initial);
                    return Done.Result;
            }

            var result = await CallBehavior(message);

            if (ReferenceEquals(result, Unhandled.Result) && Super != null)
                return await Super.Receive(message);

            return result;
        }

        internal async Task HandleBecome(Transition transition, Become message)
        {
            if (IsSuperOf(transition.From))
                return;

            if (Super != null)
                await Super.HandleBecome(transition, message);

            await CallBehavior(message);
        }

        internal async Task HandleUnbecome(Transition transition)
        {
            if (IsSuperOf(transition.To))
                return;

            await CallBehavior(Unbecome.Message);

            if (Super != null)
                await Super.HandleUnbecome(transition);
        }

        internal async Task HandleActivate(Transition transition)
        {
            if (IsSuperOf(transition.From))
                return;

            if (Super != null)
                await Super.HandleActivate(transition);

            await CallBehavior(Activate.State); 
        }

        internal async Task HandleDeactivate(Transition transition)
        {
            if (IsSuperOf(transition.To))
                return;

            await CallBehavior(Deactivate.State);

            if (Super != null)
                await Super.HandleDeactivate(transition);
        }

        Task<object> CallBehavior(object message)
        {
            var task = Behavior(message);
            
            if (task == null)
                throw new InvalidOperationException($"Behavior returns null task on handling '{message}' message");

            return task;
        }

        public static implicit operator string(State state) => state.Name;

        bool IsSuperOf(State state) => state?.IsSubstateOf(this) ?? false;
        bool IsSubstateOf(State state) => IsSubstateOf(state.Name);

        public bool IsSubstateOf(string state) => 
            Super?.Name == state || (Super?.IsSubstateOf(state) ?? false);

        internal string ToDebugString() => Super != null 
            ? $"[{Name}]->{Super.ToDebugString()}"
            : $"[{Name}]";

        public override string ToString() => Name;
    }
}