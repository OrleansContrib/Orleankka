using System;
using System.Linq;

using Orleankka.Core;

namespace Orleankka.TestKit
{
    using Core.Streams;

    public static class StreamSubscriptionSpecification<T> where T : Actor
    {
        public static bool Matches(string stream, string target)
        {
            var actor = ActorType.From(typeof(T));
            var proto = ActorPrototype.Define(actor);
            var specs = StreamSubscriptionSpecification.From(actor, proto);

            var matched = specs
                .Select(s => s.Match(null, stream))
                .Where(m => m != StreamSubscriptionMatch.None)
                .ToArray();

            return matched.Any(x => x.Target == target);
        }
    }
}
