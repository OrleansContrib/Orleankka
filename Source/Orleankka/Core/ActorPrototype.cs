using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Orleankka.Core
{
    using Utility;

    class ActorPrototype
    {
        static readonly Dictionary<ActorType, ActorPrototype> cache =
                    new Dictionary<ActorType, ActorPrototype>();

        readonly GC gc;
        readonly Dispatcher dispatcher;

        bool closed;

        internal static void Register(ActorType type)
        {
            cache.Add(type, Define(type.Implementation));
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

        internal static ActorPrototype Of(ActorPath path)
        {
            var prototype = cache.Find(path.Type);

            if (prototype == null)
                throw new InvalidOperationException(
                    $"Can't find implementation for path '{path}'." +
                     "Make sure you've registered assembly containing this type");

            return prototype;
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

        internal void Dispatch(Actor target, object message, Action<object> fallback)
        {
            dispatcher.Dispatch(target, message, fallback);
        }
        
        internal object DispatchResult(Actor target, object message, Func<object, object> fallback)
        {
            return dispatcher.DispatchResult(target, message, fallback);
        }
        
        internal Task<object> DispatchAsync(Actor target, object message, Func<object, Task<object>> fallback)
        {
            return dispatcher.DispatchAsync(target, message, fallback);
        }
    }
}