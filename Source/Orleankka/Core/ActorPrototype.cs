using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Orleankka.Core
{
    using Utility;

    class ActorPrototype
    {
        static readonly Dictionary<Type, ActorPrototype> cache =
                    new Dictionary<Type, ActorPrototype>();

        readonly GC gc;
        readonly Dispatcher dispatcher;

        bool closed;

        internal static void Register(Type actor)
        {
            cache.Add(actor, Define(actor));
        }

        internal static ActorPrototype Define(Type actor)
        {
            var instance = CreateInstance(actor);
            var prototype = CreatePrototype(actor);

            instance.Prototype = prototype;
            instance.Define();

            return prototype.Close();
        }

        static Actor CreateInstance(Type actor)
        {
            return (Actor) Activator.CreateInstance(actor, nonPublic: true);
        }

        static ActorPrototype CreatePrototype(Type actor)
        {
            return new ActorPrototype(actor);
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
            var prototype = cache.Find(actor);
            return prototype ?? CreatePrototype(actor);
        }

        ActorPrototype(Type actor)
        {
            gc = new GC(actor);
            dispatcher = new Dispatcher(actor);
        }

        internal void KeepAlive(ActorEndpoint endpoint)
        {
            gc.KeepAlive(endpoint);
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