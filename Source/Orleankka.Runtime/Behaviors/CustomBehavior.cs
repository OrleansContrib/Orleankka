using System;
using System.Threading.Tasks;

namespace Orleankka.Behaviors
{
    class CustomBehavior
    {
        readonly Receive receive;

        internal CustomBehavior(string name, Receive receive)
        {
            this.receive = receive;
            Name = name;
        }

        public string Name { get; }

        public Task HandleBecome(Transition transition) => 
            receive(ActorGrain.Become.Message);

        public Task HandleUnbecome(Transition transition) => 
            receive(ActorGrain.Unbecome.Message);

        public Task HandleActivate(Transition transition) => 
            receive(ActorGrain.Activate.Message);

        public Task HandleDeactivate(Transition transition) => 
            receive(ActorGrain.Deactivate.Message);

        public Task<object> HandleReceive(object message) => 
            receive(message);
    }
}
