using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace Orleankka.Behaviors
{
    using Utility;

    public sealed class ActorBehavior
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

            var config = behaviors.Find(actor.GetType());
            if (config == null && !(actor.Runtime is ActorRuntime))
                config = Register(actor.GetType());

            var action = config.Find(behavior);
            if (action == null)
                throw new InvalidOperationException(
                    $"Can't find method with proper signature for behavior '{behavior}' defined on actor {actor.GetType()}. " +
                    "Should be non-generic, return Task<object> and have single 'object' parameter");

            return action;
        }

        internal static ActorBehavior Default(ActorGrain actor) => new ActorBehavior(actor);
       
        readonly ActorGrain actor;
        CustomBehavior current;

        ActorBehavior(ActorGrain actor)
        {
            this.actor = actor;
        }

        internal async Task<object> OnReceive(object message)
        {
            if (Default())
                return await actor.Dispatch(message);

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
            if (!Default())
                throw new InvalidOperationException($"Initial behavior has been already set to '{Current}'");

            var action = RegisteredAction(behavior);
            current = new CustomBehavior(behavior, x => action(actor, x));
        }

        bool Default() => current == null;

        public string Current => current?.Name;

        public Task Become(Receive behavior)
        {
            Requires.NotNull(behavior, nameof(behavior));
            return Become(behavior.Method.Name);
        }

        public async Task Become(string behavior)
        {
            if (Default())
                throw new InvalidOperationException("Initial behavior should be set before calling Become");

            if (Current == behavior)
                throw new InvalidOperationException($"Actor is already behaving as '{behavior}'");

            var action = RegisteredAction(behavior);
            var next = new CustomBehavior(behavior, x => action(actor, x));
            
            var transition = new Transition(current, next);

            try
            {
                await actor.OnTransitioning(transition);

                await current.HandleDeactivate(transition);
                await current.HandleUnbecome(transition);

                await next.HandleBecome(transition);
                await next.HandleActivate(transition);

                current = next;
                await actor.OnTransitioned(transition);
            }
            catch (Exception exception)
            {
                await actor.OnTransitionError(transition, exception);
                actor.Activation.DeactivateOnIdle();
                
                ExceptionDispatchInfo.Capture(exception).Throw();
            }
        }
    }
}