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
            Null = new CustomBehavior();

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

        readonly Dictionary<string, Func<Actor, string, Task>> onReminder = new Dictionary<string, Func<Actor, string, Task>>();
        Func<Actor, string, Task> onReminderAny;

        CustomBehavior()
        {}

        public CustomBehavior(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public void OnBecome(Func<Task> action)
        {
            if (onBecome != null)
                throw new InvalidOperationException($"OnBecome action has been already configured for behavior '{Name}'");

            onBecome = action;
        }

        public void OnUnbecome(Func<Task> action)
        {
            if (onUnbecome != null)
                throw new InvalidOperationException($"OnUnbecome action has been already configured for behavior '{Name}'");

            onUnbecome = action;
        }

        public void OnActivate(Func<Task> action)
        {
            if (onActivate != null)
                throw new InvalidOperationException($"OnActivate action has been already configured for behavior '{Name}'");

            onActivate = action;
        }

        public void OnDeactivate(Func<Task> action)
        {
            if (onDeactivate != null)
                throw new InvalidOperationException($"OnDeactivate action has been already configured for behavior '{Name}'");

            onDeactivate = action;
        }

        public void OnReceive(Func<Actor, object, Task<object>> action)
        {
            if (onReceiveAny != null)
                throw new InvalidOperationException($"OnReceive(*) action has been already configured for behavior '{Name}'");

            onReceiveAny = action;
        }

        public void OnReceive<TMessage>(Func<Actor, TMessage, Task<object>> action)
        {
            try
            {
                onReceive.Add(typeof(TMessage), (actor, message) => action(actor, (TMessage) message));
            }
            catch (ArgumentException)
            {
                throw new InvalidOperationException($"OnReceive<{typeof(TMessage)}>() action has been already configured for behavior '{Name}'");
            }
        }

        public void OnReminder(Func<Actor, string, Task> action)
        {
            if (onReminderAny != null)
                throw new InvalidOperationException($"OnReminder(*) action has been already configured for behavior '{Name}'");

            onReminderAny = action;
        }

        public void OnReminder(string id, Func<Actor, Task> action)
        {
            try
            {
                onReminder.Add(id, (a, x) => action(a));
            }
            catch (ArgumentException)
            {
                throw new InvalidOperationException($"OnReminder(\"{id}\") action has been already configured for behavior '{Name}'");
            }
        }

        public async Task HandleBecome(Transition transition)
        {
            if (IncludedIn(transition.From))
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
            if (IncludedIn(transition.From))
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

        public Task<object> HandleReceive(Actor actor, object message, RequestOrigin origin, Func<Type, object, string, RequestOrigin, Task<object>> fallback)
        {
            if (IsNull())
                return onReceiveAny(actor, message);

            var handler = TryFindReceiveHandler(message);
            if (handler != null)
                return handler.Invoke(actor, message);

            handler = TryFindReceiveAnyHandler();
            return handler != null
                       ? handler(actor, message)
                       : fallback(actor.GetType(), message, Name, origin);
        }

        Func<Actor, object, Task<object>> TryFindReceiveHandler(object message)
        {
            var handler = onReceive.Find(message.GetType());
            return handler ?? super?.TryFindReceiveHandler(message);
        }

        Func<Actor, object, Task<object>> TryFindReceiveAnyHandler() => 
            onReceiveAny ?? super?.TryFindReceiveAnyHandler();

        public Task HandleReminder(Actor actor, string id, Func<Type, string, string, Task> fallback)
        {
            if (IsNull())
                return onReminderAny(actor, id);

            var handler = TryFindReminderHandler(id);
            if (handler != null)
                return handler.Invoke(actor, id);

            handler = TryFindReminderAnyHandler();
            return handler != null
                       ? handler(actor, id)
                       : fallback(actor.GetType(), id, Name);
        }

        Func<Actor, string, Task> TryFindReminderHandler(string id)
        {
            var handler = onReminder.Find(id);
            return handler ?? super?.TryFindReminderHandler(id);
        }

        Func<Actor, string, Task> TryFindReminderAnyHandler() =>
            onReminderAny ?? super?.TryFindReminderAnyHandler();

        public void Super(CustomBehavior super)
        {
            if (this.super != null)
                throw new InvalidOperationException($"Super '{this.super.Name}' has been already configured for behavior '{Name}'");

            this.super = super;
            super.sub = this;
        }

        bool Includes(CustomBehavior behavior) => 
            this == behavior || (sub?.Includes(behavior) ?? false);

        public bool Includes(string behavior) => 
            Name == behavior || (sub?.Includes(behavior) ?? false);

        public CustomBehavior FindSuper(string name) => 
            Name == name ? this : super?.FindSuper(name);

        bool IncludedIn(CustomBehavior behavior) => 
            behavior?.FindSuper(Name) != null;

        string ToDebugString() => 
            super != null ? $"[{Name}]" + "->" + super.ToDebugString() : $"[{Name}]";

        bool IsNull() => ReferenceEquals(this, Null);
    }
}
