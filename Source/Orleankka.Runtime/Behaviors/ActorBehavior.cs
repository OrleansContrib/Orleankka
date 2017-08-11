using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Orleankka.Behaviors
{
    using Services;
    using Utility;

    public sealed class ActorBehavior
    {
        static readonly Dictionary<Type, Dictionary<string, Action<object>>> behaviors =
                    new Dictionary<Type, Dictionary<string, Action<object>>>();

        internal static Dictionary<string, Action<object>> Register(Type actor)
        {
            Requires.NotNull(actor, nameof(actor));

            var config = new Dictionary<string, Action<object>>();
            var current = actor;

            while (current != typeof(Actor))
            {
                Debug.Assert(current != null);

                const BindingFlags scope = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
                foreach (var method in current.GetMethods(scope).Where(IsBehavioral))
                {
                    if (method.ReturnType != typeof(void) ||
                        method.GetGenericArguments().Length != 0 ||
                        method.GetParameters().Length != 0)
                        throw new InvalidOperationException($"Behavior method '{method.Name}' defined on '{current}' has incorrent signature. " +
                                                            "Should be void, non-generic and parameterless");

                    var target = Expression.Parameter(typeof(object));
                    var call = Expression.Call(Expression.Convert(target, actor), method);
                    var action = Expression.Lambda<Action<object>>(call, target).Compile();

                    config[method.Name] = action;
                }
                current = current.BaseType;
            }

            if (config.Count > 0)
            {
                var constructor = actor.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
                if (constructor == null)
                    throw new InvalidOperationException($"Actor type '{actor}' should have parameterless constructor " +
                                                        "in order to use behaviors functionality");
            }

            behaviors[actor] = config;
            return config;
        }

        static bool IsBehavioral(MethodInfo x) => 
            x.GetCustomAttribute<BehaviorAttribute>() != null || 
            x.GetCustomAttribute<TraitAttribute>() != null;

        Action<object> RegisteredAction(string behavior)
        {
            Requires.NotNull(behavior, nameof(behavior));

            var config = behaviors.Find(actor.GetType());
            if (config == null && !(actor.Runtime is ActorRuntime))
                config = Register(actor.GetType());

            var action = config.Find(behavior);
            if (action == null)
                throw new InvalidOperationException($"Can't find method with proper signature for behavior '{behavior}' defined on actor {actor.GetType()}. " +
                                                    "Should be void, non-generic and parameterless");

            return action;
        }

        internal static ActorBehavior Null(Actor actor) => new ActorBehavior(actor)
        {
            current = CustomBehavior.Null
        };
       
        internal bool mocked;
        internal readonly Actor actor;

        CustomBehavior current;
        CustomBehavior next;

        ActorBehavior(Actor actor)
        {
            this.actor = actor;
        }

        public async Task<object> Fire(object message)
        {
            Requires.NotNull(message, nameof(message));

            if (TimerService.IsExecuting() || mocked)
            {
                RequestOrigin.Store(Current);
                return await actor.Self.Ask<object>(message);
            }

            return await HandleReceive(message, new RequestOrigin(Current));
        }

        internal Task HandleActivate() => current.HandleActivate(default(Transition));
        internal Task HandleDeactivate() => current.HandleDeactivate(default(Transition));

        internal Task<object> HandleReceive(object message) => 
            HandleReceive(message, RequestOrigin.Restore());

        internal Task<object> HandleReceive(object message, RequestOrigin origin) => 
            current.HandleReceive(actor, message, origin);

        internal Task HandleReminder(string id) =>
            current.HandleReminder(actor, id);

        CustomBehavior Next
        {
            get
            {
                if (next == null)
                    throw new InvalidOperationException("Behavior can only be configured from within Become");

                return next;
            }
        }

        public void Initial(Action behavior)
        {
            Requires.NotNull(behavior, nameof(behavior));
            Initial(behavior.Method.Name);
        }

        public void Initial(string behavior)
        {
            if (!IsNull())
                throw new InvalidOperationException($"Initial behavior has been already set to '{Current}'");

            var action = RegisteredAction(behavior);
            next = new CustomBehavior(behavior);
            action(actor);

            current = next;
            next = null;
        }

        bool IsNull() => ReferenceEquals(current, CustomBehavior.Null);

        public string Current => current.Name;

        public Task Become(Action behavior)
        {
            Requires.NotNull(behavior, nameof(behavior));
            return Become(behavior.Method.Name);
        }

        public async Task Become(string behavior)
        {
            if (IsNull())
                throw new InvalidOperationException("Initial behavior should be set before calling Become");

            if (next != null)
                throw new InvalidOperationException($"Become cannot be called again while behavior configuration is in progress.\n" +
                                                    $"Current: {Current}, In-progress: {next.Name}, Offending: {behavior}");

            if (Current == behavior)
                throw new InvalidOperationException($"Actor is already behaving as '{behavior}'");

            if (TimerService.IsExecuting())
                throw new InvalidOperationException($"Can't switch to '{behavior}' behavior. Switching behaviors from inside timer callback is unsafe. " +
                                                     "Use Fire() to send a message and then call Become inside message handler");

            var action = RegisteredAction(behavior);
            next = new CustomBehavior(behavior);
            action(actor);
            
            var transition = new Transition(current, next);

            await current.HandleDeactivate(transition);
            await current.HandleUnbecome(transition);

            current = next;
            await current.HandleBecome(transition);

            next = null; // now become could be re-entered and we signal about transition
            await actor.OnTransitioned(transition.To.Name, transition.From.Name);

            await current.HandleActivate(transition);            
        }

        public void Super(Action behavior)
        {
            Requires.NotNull(behavior, nameof(behavior));
            Super(behavior.Method.Name);
        }

        public void Super(string behavior)
        {
            if (next == null)
                throw new InvalidOperationException($"Super behavior can only be specified while behavior configuration is in progress. " +
                                                    $"Current: {Current}, Offending: {behavior}");

            if (next.Includes(behavior))
                throw new InvalidOperationException("Detected cyclic declaration of super behaviors. " +
                                                   $"'{behavior}' is already within super chain of {next.Name}");

            var existent = current.FindSuper(behavior);
            if (existent != null)
            {
                next.Super(existent);
                return;
            }

            var prev = next;
            next = new CustomBehavior(behavior);

            prev.Super(next);
            var action = RegisteredAction(behavior);
            action(actor);

            next = prev;
        }

        public void Trait(params Action[] traits)
        {
            Requires.NotNull(traits, nameof(traits));

            if (traits.Any(x => x == null))
                throw new ArgumentException("Given parameter array contains null value", nameof(traits));

            Trait(traits.Select(x => x.Method.Name).ToArray());
        }

        public void Trait(params string[] traits)
        {
            Requires.NotNull(traits, nameof(traits));

            if (traits.Any(string.IsNullOrWhiteSpace))
                throw new ArgumentException("Given parameter array contains null/empty value", nameof(traits));

            foreach (var trait in traits)
            {
                var action = RegisteredAction(trait);
                action(actor);
            }
        }

        public void OnBecome(Action action)
        {
            Requires.NotNull(action, nameof(action));
            OnBecome(() =>
            {
                action();
                return Task.CompletedTask;
            });
        }

        public void OnBecome(Func<Task> action)
        {
            Requires.NotNull(action, nameof(action));
            Next.OnBecome(action);
        }

        public void OnUnbecome(Action action)
        {
            Requires.NotNull(action, nameof(action));
            OnUnbecome(() =>
            {
                action();
                return Task.CompletedTask;
            });
        }

        public void OnUnbecome(Func<Task> action)
        {
            Requires.NotNull(action, nameof(action));
            Next.OnUnbecome(action);
        }

        public void OnReceive<TMessage>(Action<TMessage> action)
        {
            Requires.NotNull(action, nameof(action));
            OnReceive<TMessage>(x =>
            {
                action(x);
                return Task.CompletedTask;
            });
        }

        public void OnReceive<TMessage, TResult>(Func<TMessage, TResult> action)
        {
            Requires.NotNull(action, nameof(action));
            Next.OnReceive<TMessage>((a, m) => Task.FromResult((object) action(m)));
        }

        public void OnReceive<TMessage>(Func<TMessage, Task> action)
        {
            Requires.NotNull(action, nameof(action));
            Next.OnReceive<TMessage>(async (a, m) =>
            {
                await action(m);
                return null;
            });
        }

        public void OnReceive<TMessage>(Func<TMessage, Task<object>> action)
        {
            Requires.NotNull(action, nameof(action));
            Next.OnReceive<TMessage>((a, m) => action(m));
        }

        public void OnReceive<TMessage, TResult>(Func<TMessage, Task<TResult>> action)
        {
            Requires.NotNull(action, nameof(action));
            Next.OnReceive<TMessage>(async (a, m) => await action(m));
        }

        public void OnReceive(Action<object> action)
        {
            Requires.NotNull(action, nameof(action));
            OnReceive(x =>
            {
                action(x);
                return Task.CompletedTask;
            });
        }

        public void OnReceive(Func<object, Task> action)
        {
            Requires.NotNull(action, nameof(action));
            Next.OnReceive(async (a, m) =>
            {
                await action(m);
                return null;
            });
        }

        public void OnReceive(Func<object, Task<object>> action)
        {
            Requires.NotNull(action, nameof(action));
            Next.OnReceive((a, m) => action(m));
        }

        public void OnActivate(Action action)
        {
            Requires.NotNull(action, nameof(action));
            OnActivate(() =>
            {
                action();
                return Task.CompletedTask;
            });
        }

        public void OnActivate(Func<Task> action)
        {
            Requires.NotNull(action, nameof(action));
            Next.OnActivate(action);
        }

        public void OnDeactivate(Action action)
        {
            Requires.NotNull(action, nameof(action));
            OnDeactivate(() =>
            {
                action();
                return Task.CompletedTask;
            });
        }

        public void OnDeactivate(Func<Task> action)
        {
            Requires.NotNull(action, nameof(action));
            Next.OnDeactivate(action);
        }

        public void OnReminder(string id, Action action)
        {
            Requires.NotNullOrWhitespace(id, nameof(id));
            Requires.NotNull(action, nameof(action));
            OnReminder(id, () =>
            {
                action();
                return Task.CompletedTask;
            });
        }

        public void OnReminder(string id, Func<Task> action)
        {
            Requires.NotNullOrWhitespace(id, nameof(id));
            Requires.NotNull(action, nameof(action));
            Next.OnReminder(id, a => action());
        }

        public void OnReminder(Action<string> action)
        {
            Requires.NotNull(action, nameof(action));
            OnReminder(x =>
            {
                action(x);
                return Task.CompletedTask;
            });
        }

        public void OnReminder(Func<string, Task> action)
        {
            Requires.NotNull(action, nameof(action));
            Next.OnReminder((a, id) => action(id));
        }
    }
}