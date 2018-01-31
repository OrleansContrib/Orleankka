using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Orleankka.Behaviors
{
    using Utility;

    public sealed class Behavior
    {
        static readonly Dictionary<Type, Dictionary<string, Func<object, object, Task<object>>>> behaviors =
                    new Dictionary<Type, Dictionary<string, Func<object, object, Task<object>>>>();

        internal static Dictionary<string, Func<object, object, Task<object>>> Register(Type actor)
        {
            Requires.NotNull(actor, nameof(actor));

            var config = new Dictionary<string, Func<object, object, Task<object>>>();
            var current = actor;

            while (current != typeof(ActorGrain) && current != null)
            {
                const BindingFlags scope = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
                foreach (var method in current.GetMethods(scope).Where(IsBehavioral))
                {
                    if (method.ReturnType != typeof(Task<object>) ||
                        method.GetGenericArguments().Length != 0 ||
                        method.GetParameters().Length != 1 || 
                        method.GetParameters()[0].ParameterType != typeof(object))
                        throw new InvalidOperationException($"Behavior method '{method.Name}' defined on '{current}' has incorrent signature. " +
                                                            "Should be non-generic, return Task<object> and have single 'object' parameter");

                    var target = Expression.Parameter(typeof(object));
                    var request = Expression.Parameter(typeof(object));

                    var call = Expression.Call(Expression.Convert(target, actor), method, request);
                    var action = Expression.Lambda<Func<object, object, Task<object>>>(call, target, request).Compile();

                    config[method.Name] = action;
                }

                current = current.BaseType;
            }

            behaviors[actor] = config;
            return config;
        }

        static bool IsBehavioral(MethodInfo x) => x.GetCustomAttribute<BehaviorAttribute>() != null;

        Func<object, object, Task<object>> RegisteredAction(string behavior)
        {
            Requires.NotNull(behavior, nameof(behavior));

            var config = behaviors.Find(actor.GetType()) 
                         ?? Register(actor.GetType());

            var action = config.Find(behavior);
            if (action == null)
                throw new InvalidOperationException(
                    $"Can't find method with proper signature for behavior '{behavior}' defined on actor {actor.GetType()}. " +
                    "Should be non-generic, return Task<object> and have single 'object' parameter");

            return action;
        }

        readonly ActorGrain actor;
        CustomBehavior current;

        public Behavior(ActorGrain actor)
        {
            Requires.NotNull(actor, nameof(actor));
            this.actor = actor;
        }

        public Behavior(ActorGrain actor, string initial)
        {
            Requires.NotNull(actor, nameof(initial));
            Requires.NotNull(initial, nameof(initial));

            this.actor = actor;
            Initial(initial);
        }

        public Behavior(ActorGrain actor, Receive initial)
        {
            Requires.NotNull(actor, nameof(initial));
            Requires.NotNull(initial, nameof(initial));

            this.actor = actor;
            Initial(initial.Method.Name);
        }

        public Func<Transition, Task> OnTransitioning { get; set; } = t => Task.CompletedTask;
        public Func<Transition, Task> OnTransitioned  { get; set; } = t => Task.CompletedTask;
        public Func<Transition, Exception, Task> OnTransitionError { get; set; } = (t, e) => Task.CompletedTask;

        public async Task<object> OnReceive(object message)
        {
            if (!Initialized())
                throw new InvalidOperationException($"Initial behavior should be set for actor '{actor}' in order to receive messages");

            switch (message)
            {
                case ActorGrain.Activate _:
                    await current.HandleActivate(default(Transition));
                    return ActorGrain.Done; 
                
                case ActorGrain.Deactivate _:
                    await current.HandleDeactivate(default(Transition));
                    return ActorGrain.Done;
            }

            return await current.HandleReceive(message);
        }

        public void Initial(Receive behavior)
        {
            Requires.NotNull(behavior, nameof(behavior));
            Initial(behavior.Method.Name);
        }

        public void Initial(string behavior)
        {
            if (Initialized())
                throw new InvalidOperationException($"Initial behavior has been already set to '{Current}'");

            var action = RegisteredAction(behavior);
            current = new CustomBehavior(behavior, x => action(actor, x));
        }

        public string Current => current?.Name;
        bool Initialized() => current != null;

        public Task Become(Receive behavior)
        {
            Requires.NotNull(behavior, nameof(behavior));
            return Become(behavior.Method.Name);
        }

        public async Task Become(string behavior)
        {
            if (!Initialized())
                throw new InvalidOperationException($"Initial behavior should be set for actor '{actor}' before calling Become");

            if (Current == behavior)
                throw new InvalidOperationException($"Actor '{actor}' is already behaving as '{behavior}'");

            var action = RegisteredAction(behavior);
            var next = new CustomBehavior(behavior, x => action(actor, x));
            
            var transition = new Transition(current, next);

            try
            {
                await OnTransitioning(transition);

                await current.HandleDeactivate(transition);
                await current.HandleUnbecome(transition);

                await next.HandleBecome(transition);
                await next.HandleActivate(transition);

                current = next;
                await OnTransitioned(transition);
            }
            catch (Exception exception)
            {
                await OnTransitionError(transition, exception);
            }
        }
    }
}