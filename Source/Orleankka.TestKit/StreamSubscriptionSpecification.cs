using System.Linq;

namespace Orleankka.TestKit
{
    using Core.Streams;

    public static class StreamSubscriptionSpecification<T> where T : Actor
    {
        public static bool Matches(string stream, string target)
        {
            var specs = StreamSubscriptionSpecification.From(type, type.Implementation);

            var matched = specs
                .Select(s => s.Match(null, stream))
                .Where(m => m != StreamSubscriptionMatch.None)
                .ToArray();

            return matched.Any(x => x.Target == target);
        }
    }
}
