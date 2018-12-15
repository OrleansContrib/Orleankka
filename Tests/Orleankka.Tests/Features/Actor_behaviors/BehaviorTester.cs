using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
            Task<object> Record(object x)
            {
                RecordTransitions(name, x);
                return TaskResult.Done;
            }

            if (super != null)
                machine.State(name, Record, super);
            else
                machine.State(name, Record);

            return this;
        }

        public BehaviorTester State(Receive receive, Func<Receive, Receive> extend = null) => 
            State(receive, null, extend);

        public BehaviorTester State(Receive receive, Receive super, Func<Receive, Receive> extend = null) => 
            State(receive.Method.Name, super?.Method.Name, receive, extend);

        public BehaviorTester State(string name, Receive receive) => 
            State(name, null, receive);

        public BehaviorTester State(string name, string super, Receive receive, Func<Receive, Receive> extend = null)
        {
            Task<object> Record(object x)
            {
                RecordTransitions(name, x);
                return receive(x);
            }

            if (super != null)
                machine.State(name, Record, super, extend);
            else
                machine.State(name, Record, extend);

            return this;
        }

        public BehaviorTester Initial(string name)
        {
            initial = name;
            return this;
        }

        public BehaviorTester Initial(Receive behavior)
        {
            initial = behavior.Method.Name;
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