using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Orleankka
{
    using Core;
    using Utility;

    [DebuggerDisplay("_{actor}")]
    class ActorPrototype
    {
        static readonly Dictionary<Type, ActorPrototype> cache =
                    new Dictionary<Type, ActorPrototype>();

        readonly GC gc;
        readonly Reentrant reentrant;
        readonly Dispatcher dispatcher;

        bool closed;

        internal static void Register(Type actor)
        {
            var prototype = new ActorPrototype(actor);

            var instance = (Actor) Activator.CreateInstance(actor, nonPublic: true);
            instance.Prototype = prototype;
            instance.Define();

            cache.Add(actor, prototype.Close());
        }

        ActorPrototype Close()
        {
            closed = true;
            return this;
        }

        internal static void Reset()
        {
            cache.Clear();
        }

        internal static ActorPrototype Of(Type actor)
        {
            ActorPrototype prototype = cache.Find(actor);
            return prototype ?? new ActorPrototype(actor);
        }

        ActorPrototype(Type actor)
        {
            gc = new GC(actor);
            reentrant = new Reentrant(actor);
            dispatcher = new Dispatcher(actor);
        }

        internal void SetKeepAlive(TimeSpan timeout)
        {
            gc.SetKeepAlive(timeout);
        }

        internal void KeepAlive(IActorEndpointActivationService endpoint)
        {
            gc.KeepAlive(endpoint);
        }

        internal void RegisterReentrant(Func<object, bool> evaluator)
        {
            AssertClosed();
            reentrant.Register(evaluator);
        }

        internal bool IsReentrant(object message)
        {
            return reentrant.IsReentrant(message);
        }

        internal void RegisterHandler(MethodInfo method)
        {
            AssertClosed();
            dispatcher.Register(method);
        }

        void AssertClosed()
        {
            if (closed)
                throw new InvalidOperationException("Actor prototype can only be defined within Define() method");
        }

        internal void Dispatch(Actor target, object message)
        {
            dispatcher.Dispatch(target, message);
        }
        
        internal object DispatchResult(Actor target, object message)
        {
            return dispatcher.DispatchResult(target, message);
        }
        
        internal Task<object> DispatchAsync(Actor target, object message)
        {
            return dispatcher.DispatchAsync(target, message);
        }
    }
}