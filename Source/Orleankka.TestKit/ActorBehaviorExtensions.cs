using System;
using System.Threading.Tasks;

namespace Orleankka.TestKit
{
    using Behaviors;

    public static class ActorBehaviorExtensions
    {
        public static ActorRefMock Mock(this ActorBehavior behavior)
        {
            behavior.mocked = true;

            var self = behavior.actor.Self as ActorRefMock;
            if (self == null)
                throw new InvalidOperationException("Actor runtime need to be mocked as well");

            return self;
        }

        public static async Task Activate(this Actor actor, Action behavior) => await Activate(actor, behavior.Method.Name);

        public static async Task Activate(this Actor actor, string behavior)
        {
            actor.Behavior.Initial(behavior);
            await Activate(actor.Behavior);
        }

        public static async Task Activate(this ActorBehavior behavior) => await behavior.HandleActivate();
        public static async Task Deactivate(this ActorBehavior behavior) => await behavior.HandleDeactivate();
        public static async Task<object> Receive(this ActorBehavior behavior, object message) => await behavior.HandleReceive(message);
        public static async Task Reminder(this ActorBehavior behavior, string id) => await behavior.HandleReminder(id);
    }
}