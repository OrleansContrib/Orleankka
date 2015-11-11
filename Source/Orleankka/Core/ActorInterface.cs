using System;
using System.Collections.Generic;

namespace Orleankka.Core
{
    using Utility;

    class ActorInterface
    {
        static readonly Dictionary<ActorType, ActorInterface> cache =
                    new Dictionary<ActorType, ActorInterface>();

        readonly Reentrant reentrant;

        internal static void Register(ActorType actor)
        {
            cache.Add(actor, new ActorInterface(actor.Interface));
        }

        internal static void Reset()
        {
            cache.Clear();
        }

        internal static ActorInterface Of(ActorPath path)
        {
            var type = ActorType.Registered(path.Code);

            var @interface = cache.Find(type);
            if (@interface == null)
                throw new InvalidOperationException(
                    $"Can't find interface for path '{path}'." +
                     "Make sure you've registered assembly containing this type");

            return @interface;
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