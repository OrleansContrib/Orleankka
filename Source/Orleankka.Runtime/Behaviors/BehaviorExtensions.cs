using System.Threading.Tasks;

namespace Orleankka.Behaviors
{
    public static class BehaviorExtensions
    {
        public static Task Become(this ActorGrain actor, string behavior) => actor.Behavior.Become(behavior);
        public static Task Become(this ActorGrain actor, Receive behavior) => actor.Behavior.Become(behavior);
    }
}