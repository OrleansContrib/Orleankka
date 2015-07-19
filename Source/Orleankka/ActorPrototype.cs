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
    public class ActorPrototype
    {
        static readonly Dictionary<Type, ActorPrototype> cache =
                    new Dictionary<Type, ActorPrototype>();

        readonly GC gc;
        readonly Reentrant reentrant;
        readonly Dispatcher dispatcher;

        bool closed;

        internal static void Register(Type actor)
        {
            var instance  = CreateInstance(actor);
            var prototype = CreatePrototype(instance);
            
            instance._ = prototype;
            instance.Define();

            cache.Add(actor, prototype.Close());
        }

        static Actor CreateInstance(Type actor)
        {
            return (Actor) Activator.CreateInstance(actor, nonPublic: true);
        }

        static ActorPrototype CreatePrototype(Actor actor)
        {
            return (ActorPrototype) Activator.CreateInstance(actor.Prototype, new object[]{actor.GetType()});
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
            return prototype ?? CreatePrototype(CreateInstance(actor));
        }

        public ActorPrototype(Type actor)
        {
            gc = new GC(actor);
            reentrant = new Reentrant(actor);
            dispatcher = new Dispatcher(actor);
        }

        internal void SetKeepAlive(TimeSpan timeout)
        {
            gc.SetKeepAlive(timeout);
        }

        internal void KeepAlive(ActorEndpoint endpoint)
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