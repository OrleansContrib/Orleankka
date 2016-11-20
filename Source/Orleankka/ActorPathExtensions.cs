using System;
using System.Linq;

using Orleankka.Utility;

namespace Orleankka
{
    public static class ActorPathExtensions
    {
        public static ActorPath ToActorPath(this Type type, string id)
        {
            Requires.NotNull(type, nameof(type));
            var key = ActorTypeName.Of(type);
            return ActorPath.From(key, id);
        }
    }
}