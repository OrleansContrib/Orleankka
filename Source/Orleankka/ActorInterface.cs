using System;
using System.Collections.Generic;
using System.Linq;

namespace Orleankka
{
    using Utility;

    class ActorInterface
    {
        static readonly Dictionary<Type, ActorInterface> cache =
                    new Dictionary<Type, ActorInterface>();

        readonly Reentrant reentrant;

        internal static void Register(Type actor)
        {
            cache.Add(actor, new ActorInterface(actor));
        }

        internal static void Reset()
        {
            cache.Clear();
        }

        internal static ActorInterface Of(Type actor)
        {
            var prototype = cache.Find(actor);
            return prototype ?? new ActorInterface(actor);
        }

        ActorInterface(Type actor)
        {
            reentrant = new Reentrant(actor);
        }

        internal bool IsReentrant(object message)
        {
            return reentrant.IsReentrant(message);
        }
    }
}