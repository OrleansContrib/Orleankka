using System;
using System.Threading.Tasks;

namespace Orleankka.Behaviors
{
    public static class BehaviorExtensions
    {
        public static Task<object> Fire(this ActorGrain actor, object message) => actor.Behavior.Fire(message);

        public static Task Become(this ActorGrain actor, string behavior) => actor.Behavior.Become(behavior);
        public static Task Become(this ActorGrain actor, Action behavior) => actor.Behavior.Become(behavior);

        public static void Super(this ActorGrain actor, string behavior) => actor.Behavior.Super(behavior);
        public static void Super(this ActorGrain actor, Action behavior) => actor.Behavior.Super(behavior);

        public static void Trait(this ActorGrain actor, params string[] traits) => actor.Behavior.Trait(traits);
        public static void Trait(this ActorGrain actor, params Action[] traits) => actor.Behavior.Trait(traits);

        public static void OnBecome(this ActorGrain actor, Action action) => actor.Behavior.OnBecome(action);
        public static void OnBecome(this ActorGrain actor, Func<Task> action) => actor.Behavior.OnBecome(action);

        public static void OnUnbecome(this ActorGrain actor, Action action) => actor.Behavior.OnUnbecome(action);
        public static void OnUnbecome(this ActorGrain actor, Func<Task> action) => actor.Behavior.OnUnbecome(action);

        public static void OnActivate(this ActorGrain actor, Action action) => actor.Behavior.OnActivate(action);
        public static void OnActivate(this ActorGrain actor, Func<Task> action) => actor.Behavior.OnActivate(action);

        public static void OnDeactivate(this ActorGrain actor, Action action) => actor.Behavior.OnDeactivate(action);
        public static void OnDeactivate(this ActorGrain actor, Func<Task> action) => actor.Behavior.OnDeactivate(action);

        public static void OnReminder(this ActorGrain actor, string id, Action action) => actor.Behavior.OnReminder(id, action);
        public static void OnReminder(this ActorGrain actor, string id, Func<Task> action) => actor.Behavior.OnReminder(id, action);
        public static void OnReminder(this ActorGrain actor, Action<string> action) => actor.Behavior.OnReminder(action);
        public static void OnReminder(this ActorGrain actor, Func<string, Task> action) => actor.Behavior.OnReminder(action);

        public static void OnReceive<TMessage>(this ActorGrain actor, Action<TMessage> action) => actor.Behavior.OnReceive(action);
        public static void OnReceive<TMessage, TResult>(this ActorGrain actor, Func<TMessage, TResult> action) => actor.Behavior.OnReceive(action);
        public static void OnReceive<TMessage>(this ActorGrain actor, Func<TMessage, Task> action) => actor.Behavior.OnReceive(action);
        public static void OnReceive<TMessage>(this ActorGrain actor, Func<TMessage, Task<object>> action) => actor.Behavior.OnReceive(action);
        public static void OnReceive<TMessage, TResult>(this ActorGrain actor, Func<TMessage, Task<TResult>> action) => actor.Behavior.OnReceive(action);
        public static void OnReceive(this ActorGrain actor, Action<object> action) => actor.Behavior.OnReceive(action);
        public static void OnReceive(this ActorGrain actor, Func<object, Task> action) => actor.Behavior.OnReceive(action);
    }
}