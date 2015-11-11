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
            var prototype = CreatePrototype(actor);
            return prototype.Close();
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
            var type = ActorType.Registered(path.Code);

            var prototype = cache.Find(type);
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

        internal Task<object> Dispatch(Actor target, object message, Func<object, Task<object>> fallback)
        {
            return dispatcher.Dispatch(target, message, fallback);
        }
    }
}