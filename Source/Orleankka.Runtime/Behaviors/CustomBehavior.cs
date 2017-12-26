using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

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

            Null.OnActivate(() => Task.CompletedTask);
            Null.OnDeactivate(() => Task.CompletedTask);
            Null.OnReceive((actor, message) => actor.Dispatch(message));
            Null.OnReminder((actor, id) => 
            {
                throw new NotImplementedException(
                    $"Override {nameof(ActorGrain.OnReminder)}(string id) method in " +
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

        readonly Dictionary<Type, Func<ActorGrain, object, Task<object>>> onReceive = new Dictionary<Type, Func<ActorGrain, object, Task<object>>>();
        Func<ActorGrain, object, Task<object>> onReceiveAny;

        readonly Dictionary<string, Func<ActorGrain, string, Task>> onReminder = new Dictionary<string, Func<ActorGrain, string, Task>>();
        Func<ActorGrain, string, Task> onReminderAny;

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

        public void OnReceive(Func<ActorGrain, object, Task<object>> action)
        {
            if (onReceiveAny != null)
                throw new InvalidOperationException($"OnReceive(*) action has been already configured for behavior '{Name}'");

            onReceiveAny = action;
        }

        public void OnReceive<TMessage>(Func<ActorGrain, TMessage, Task<object>> action)
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

        public void OnReminder(Func<ActorGrain, string, Task> action)
        {
            if (onReminderAny != null)
                throw new InvalidOperationException($"OnReminder(*) action has been already configured for behavior '{Name}'");

            onReminderAny = action;
        }

        public void OnReminder(string id, Func<ActorGrain, Task> action)
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
            if (IncludedIn(transition.from))
                return;

            if (super != null)
                await super.HandleBecome(transition);

            if (onBecome != null)
                await onBecome();
        }

        public async Task HandleUnbecome(Transition transition)
        {
            if (Includes(transition.to))
                return;

            if (onUnbecome != null)
                await onUnbecome();

            if (super != null)
                await super.HandleUnbecome(transition);
        }

        public async Task HandleActivate(Transition transition)
        {
            if (IncludedIn(transition.from))
                return;

            if (super != null)
                await super.HandleActivate(transition);

            if (onActivate != null)
                await onActivate();
        }

        public async Task HandleDeactivate(Transition transition)
        {
            if (Includes(transition.to))
                return;

            if (onDeactivate != null)
                await onDeactivate();

            if (super != null)
                await super.HandleDeactivate(transition);
        }

        public Task<object> HandleReceive(ActorGrain actor, object message, RequestOrigin origin)
        {
            if (IsNull())
                return onReceiveAny(actor, message);

            var handler = TryFindReceiveHandler(message);
            if (handler != null)
                return handler.Invoke(actor, message);

            handler = TryFindReceiveAnyHandler();
            return handler != null
                       ? handler(actor, message)
                       : actor.OnUnhandledReceive(origin, message);
        }

        Func<ActorGrain, object, Task<object>> TryFindReceiveHandler(object message)
        {
            var handler = onReceive.Find(message.GetType());
            return handler ?? super?.TryFindReceiveHandler(message);
        }

        Func<ActorGrain, object, Task<object>> TryFindReceiveAnyHandler() => 
            onReceiveAny ?? super?.TryFindReceiveAnyHandler();

        public Task HandleReminder(ActorGrain actor, string id)
        {
            if (IsNull())
                return onReminderAny(actor, id);

            var handler = TryFindReminderHandler(id);
            if (handler != null)
                return handler.Invoke(actor, id);

            handler = TryFindReminderAnyHandler();
            return handler != null
                       ? handler(actor, id)
                       : actor.OnUnhandledReminder(id);
        }

        Func<ActorGrain, string, Task> TryFindReminderHandler(string id)
        {
            var handler = onReminder.Find(id);
            return handler ?? super?.TryFindReminderHandler(id);
        }

        Func<ActorGrain, string, Task> TryFindReminderAnyHandler() =>
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
