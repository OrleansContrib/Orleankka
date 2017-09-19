using System;
using System.Threading.Tasks;

namespace Orleankka.TestKit
{
    using Behaviors;

    public static class ActorBehaviorExtensions
    {
        public static void Mock(this ActorBehavior behavior)
        {
            behavior.mocked = true;

            var self = behavior.actor.Self as ActorRefMock;
            if (self == null)
                throw new InvalidOperationException("Actor runtime should be a mock as well");
        }

        public static async Task Activate(this ActorBehavior behavior, Action action) => await Activate(behavior, action.Method.Name);

        public static async Task Activate(this ActorBehavior behavior, string name)
        {
            behavior.Initial(name);
            await Activate(behavior);
        }

        public static async Task Activate(this ActorBehavior behavior) => await behavior.HandleActivate();
        public static async Task Deactivate(this ActorBehavior behavior) => await behavior.HandleDeactivate();
        public static async Task<object> Receive(this ActorBehavior behavior, object message) => await behavior.HandleReceive(message);
        public static async Task Reminder(this ActorBehavior behavior, string id) => await behavior.HandleReminder(id);
    }
}