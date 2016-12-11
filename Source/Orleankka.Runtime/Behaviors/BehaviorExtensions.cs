using System;
using System.Threading.Tasks;

namespace Orleankka.Behaviors
{
    public static class BehaviorExtensions
    {
        public static Task Become(this Actor actor, Action behavior) => actor.Behavior.Become(behavior);
        public static void Super(this Actor actor, Action behavior) => actor.Behavior.Super(behavior);

        public static void OnBecome(this Actor actor, Action action) => actor.Behavior.OnBecome(action);
        public static void OnBecome(this Actor actor, Func<Task> action) => actor.Behavior.OnBecome(action);

        public static void OnUnbecome(this Actor actor, Action action) => actor.Behavior.OnUnbecome(action);
        public static void OnUnbecome(this Actor actor, Func<Task> action) => actor.Behavior.OnUnbecome(action);

        public static void OnActivate(this Actor actor, Action action) => actor.Behavior.OnActivate(action);
        public static void OnActivate(this Actor actor, Func<Task> action) => actor.Behavior.OnActivate(action);

        public static void OnDeactivate(this Actor actor, Action action) => actor.Behavior.OnDeactivate(action);
        public static void OnDeactivate(this Actor actor, Func<Task> action) => actor.Behavior.OnDeactivate(action);

        public static void OnReminder(this Actor actor, string id, Action action) => actor.Behavior.OnReminder(id, action);
        public static void OnReminder(this Actor actor, string id, Func<Task> action) => actor.Behavior.OnReminder(id, action);
        public static void OnReminder(this Actor actor, Action<string> action) => actor.Behavior.OnReminder(action);
        public static void OnReminder(this Actor actor, Func<string, Task> action) => actor.Behavior.OnReminder(action);

        public static void On<TMessage>(this Actor actor, Action<TMessage> action) => actor.Behavior.On(action);
        public static void On<TMessage, TResult>(this Actor actor, Func<TMessage, TResult> action) => actor.Behavior.On(action);
        public static void On<TMessage>(this Actor actor, Func<TMessage, Task> action) => actor.Behavior.On(action);
        public static void On<TMessage>(this Actor actor, Func<TMessage, Task<object>> action) => actor.Behavior.On(action);
        public static void On<TMessage, TResult>(this Actor actor, Func<TMessage, Task<TResult>> action) => actor.Behavior.On(action);
        public static void On(this Actor actor, Action<object> action) => actor.Behavior.On(action);
        public static void On(this Actor actor, Func<object, Task> action) => actor.Behavior.On(action);
    }
}