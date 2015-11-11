using System;
using System.Collections.Generic;
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

        internal static void Register(ActorType type) => cache.Add(type, Define(type.Implementation));
        internal static void Reset()                  => cache.Clear();

        internal static ActorPrototype Define(Type actor) => CreatePrototype(actor);
        static ActorPrototype CreatePrototype(Type actor) => new ActorPrototype(actor);

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
            => gc.KeepAlive(endpoint);

        internal Task<object> Dispatch(Actor target, object message, Func<object, Task<object>> fallback) 
            => dispatcher.Dispatch(target, message, fallback);
    }
}