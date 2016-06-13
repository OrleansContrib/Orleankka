using System;

using Orleankka.Utility;

namespace Orleankka.CSharp
{
    public static class ActorPathExtensions
    {
        public static ActorPath ToActorPath(this Type type, string id)
        {
            Requires.NotNull(type, nameof(type));
            var code = ActorTypeCode.Of(type);
            return ActorPath.From(code, id);
        }
    }
}