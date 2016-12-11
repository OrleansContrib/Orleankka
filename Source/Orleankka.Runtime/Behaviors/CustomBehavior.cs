using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

using Orleans;

namespace Orleankka.Behaviors
{
    using Utility;

    [DebuggerDisplay("{ToDebugString()}")]
    class CustomBehavior
    {
        internal static readonly CustomBehavior Null;

        static CustomBehavior()
        {
            Null = new CustomBehavior("<NULL>");

            Null.OnActivate(() => TaskDone.Done);
            Null.OnDeactivate(() => TaskDone.Done);
            Null.OnReceive((actor, message) => actor.Dispatch(message));
            Null.OnReminder((actor, id) => 
            {
                throw new NotImplementedException(
                    $"Override {nameof(Actor.OnReminder)}(string id) method in " +
                    $"class {actor.GetType()} to implement corresponding behavior");
            });

            const string warn = "Initial behavior has not been set";
            Null.OnBecome(() => { throw new InvalidOperationException(warn); });
        }

        CustomBehavior super;
        CustomBehavior sub;

        Func<Task> onBecome;
        Func<Task> onUnbecome;
        Func<Task> onActivate;
        Func<Task> onDeactivate;

        readonly Dictionary<Type, Func<Actor, object, Task<object>>> onReceive = new Dictionary<Type, Func<Actor, object, Task<object>>>();
        Func<Actor, object, Task<object>> onReceiveAny;

        readonly Dictionary<string, Func<Actor, Task>> onReminder = new Dictionary<string, Func<Actor, Task>>();
        Func<Actor, string, Task> onReminderAny;

        public CustomBehavior(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public void OnBecome(Func<Task> action) => onBecome = action;
        public void OnUnbecome(Func<Task> action) => onUnbecome = action;
        public void OnActivate(Func<Task> action) => onActivate = action;
        public void OnDeactivate(Func<Task> action) => onDeactivate = action;

        public void OnReceive(Func<Actor, object, Task<object>> action) => onReceiveAny = action;
        public void OnReceive<TMessage>(Func<Actor, TMessage, Task<object>> action) => onReceive.Add(typeof(TMessage), (actor, message) => action(actor, (TMessage)message));

        public void OnReminder(Func<Actor, string, Task> action) => onReminderAny = action;
        public void OnReminder(string id, Func<Actor, Task> action) => onReminder.Add(id, action);

        public async Task HandleBecome(Transition transition)
        {
            if (transition.Subsumes(this))
                return;

            if (super != null)
                await super.HandleBecome(transition);

            if (onBecome != null)
                await onBecome();
        }

        public async Task HandleUnbecome(Transition transition)
        {
            if (Includes(transition.To))
                return;

            if (onUnbecome != null)
                await onUnbecome();

            if (super != null)
                await super.HandleUnbecome(transition);
        }

        public async Task HandleActivate(Transition transition)
        {
            if (transition.Subsumes(this))
                return;

            if (super != null)
                await super.HandleActivate(transition);

            if (onActivate != null)
                await onActivate();
        }

        public async Task HandleDeactivate(Transition transition)
        {
            if (Includes(transition.To))
                return;

            if (onDeactivate != null)
                await onDeactivate();

            if (super != null)
                await super.HandleDeactivate(transition);
        }

        public Task<object> HandleReceive(Actor actor, object message)
        {
            if (IsNull())
                return onReceiveAny(actor, message);

            var handler = onReceive.Find(message.GetType());
            return handler != null
                       ? handler.Invoke(actor, message)
                       : onReceiveAny(actor, message);
        }

        public Task HandleReminder(Actor actor, string id)
        {
            if (IsNull())
                return onReminderAny(actor, id);

            var handler = onReminder.Find(id);
            return handler != null
                       ? handler.Invoke(actor)
                       : onReminderAny(actor, id);
        }

        public void Super(CustomBehavior super)
        {
            this.super = super;
            super.sub = this;
        }

        bool Includes(CustomBehavior behavior) => 
            this == behavior || (sub?.Includes(behavior) ?? false);

        public bool Includes(string behavior) => 
            Name == behavior || (sub?.Includes(behavior) ?? false);

        public CustomBehavior FindSuper(string name) => 
            Name == name ? this : super?.FindSuper(name);

        internal bool SuperOf(CustomBehavior behavior) => 
            FindSuper(behavior.Name) != null;

        string ToDebugString() => 
            super != null ? $"[{Name}]" + "->" + super.ToDebugString() : $"[{Name}]";

        bool IsNull() => ReferenceEquals(this, Null);
    }

    struct Transition
    {
        public readonly CustomBehavior From;
        public readonly CustomBehavior To;

        public Transition(CustomBehavior from, CustomBehavior to)
        {
            From = from;
            To = to;
        }

        public bool Subsumes(CustomBehavior behavior) => 
            From != null && From.SuperOf(behavior);
    }
}
