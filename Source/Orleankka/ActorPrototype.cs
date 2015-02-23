using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Orleankka
{
    using Utility;

    class ActorPrototype
    {
        static readonly Dictionary<Type, ActorPrototype> cache =
                    new Dictionary<Type, ActorPrototype>();

        readonly HashSet<Type> reentrant;

        internal static void Register(Type actor)
        {
            var definition = new ActorPrototype(actor);
            cache.Add(actor, definition);
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
            var attributes = actor.GetCustomAttributes<ReentrantAttribute>(inherit: true);
            reentrant = new HashSet<Type>(attributes.Select(x => x.Message));
        }

        internal bool IsReentrant(Type message)
        {
            return reentrant.Contains(message);
        }
    }
}