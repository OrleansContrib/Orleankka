using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Orleankka
{
    using Utility;

    class ActorDefinition
    {
        static readonly Dictionary<Type, ActorDefinition> cache =
            new Dictionary<Type, ActorDefinition>();

        readonly HashSet<Type> interleave;

        internal static void Register(Type actor)
        {
            var definition = new ActorDefinition(actor);
            cache.Add(actor, definition);
        }

        internal static void Reset()
        {
            cache.Clear();
        }

        internal static ActorDefinition Of(Type actor)
        {
            ActorDefinition definition = cache.Find(actor);
            return definition ?? new ActorDefinition(actor);
        }

        ActorDefinition(Type actor)
        {
            var attributes = actor.GetCustomAttributes<InterleaveAttribute>(inherit: true);
            interleave = new HashSet<Type>(attributes.Select(x => x.Message));
        }

        internal bool Interleaved(Type message)
        {
            return interleave.Contains(message);
        }
    }
}