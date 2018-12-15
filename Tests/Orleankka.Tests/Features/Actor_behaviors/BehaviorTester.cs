using System.Collections.Generic;

namespace Orleankka.Features.Actor_behaviors
{
    using Behaviors;

    class BehaviorTester
    {
        readonly StateMachine machine;
        readonly List<string> events;
        string initial;

        public BehaviorTester(List<string> events)
        {
            machine = new StateMachine();
            this.events = events;
        }

        public BehaviorTester State(string name, string super = null)
        {
            machine.State(name, x =>
            {
                RecordTransitions(name, x);
                return TaskResult.Done;
            }, 
                super);

            return this;
        }

        public BehaviorTester State(string name, Receive receive) => State(name, null, receive);

        public BehaviorTester State(string name, string super, Receive receive)
        {
            machine.State(name, x =>
            {
                RecordTransitions(name, x);
                return receive(x);
            }, 
            super);

            return this;
        }

        public BehaviorTester Initial(string name)
        {
            initial = name;
            return this;
        }

        void RecordTransitions(string behavior, object message)
        {
            switch (message)
            {
                case Become _ :
                    events.Add($"OnBecome_{behavior}");
                    break;
                case Unbecome _ :
                    events.Add($"OnUnbecome_{behavior}");
                    break;                        
                case Activate _ :
                    events.Add($"OnActivate_{behavior}");
                    break;                        
                case Deactivate _ :
                    events.Add($"OnDeactivate_{behavior}");
                    break;
            }
        }

        public static implicit operator Behavior(BehaviorTester x)
        {
            var behavior = new Behavior(x.machine.Build());

            if (x.initial != null)
                behavior.Initial(x.initial);

            return behavior;
        }
    }
}