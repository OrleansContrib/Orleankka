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
        readonly ActorType type;

        internal static void Register(ActorType type) => cache.Add(type, Define(type));
        internal static void Reset()                  => cache.Clear();

        internal static ActorPrototype Define(Type type)      => Define(ActorType.From(type));
        internal static ActorPrototype Define(ActorType type) => CreatePrototype(type);
        static ActorPrototype CreatePrototype(ActorType type) => new ActorPrototype(type);

        internal static ActorPrototype Of(string code)    => Of(ActorType.Registered(code));
        internal static ActorPrototype Of(ActorPath path) => Of(path.Code);
        internal static ActorPrototype Of(ActorType type)
        {
            var prototype = cache.Find(type);

            if (prototype == null)
                throw new InvalidOperationException(
                    $"Can't find implementation for actor '{type}'." +
                     "Make sure you've registered assembly containing this type");

            return prototype;
        }

        ActorPrototype(ActorType type)
        {
            gc = new GC(type.Implementation);
            dispatcher = new Dispatcher(type.Implementation);
            this.type = type;
        }

        public string Code => type.Code;

        internal void KeepAlive(ActorEndpoint endpoint) 
            => gc.KeepAlive(endpoint);

        internal Task<object> Dispatch(Actor target, object message, Func<object, Task<object>> fallback) 
            => dispatcher.Dispatch(target, message, fallback);

        internal bool DeclaresHandlerFor(Type message)
            => dispatcher.HasRegisteredHandler(message);
    }
}