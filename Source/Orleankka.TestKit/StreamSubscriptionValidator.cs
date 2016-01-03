
using Orleankka.Core.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orleankka.TestKit
{
    public static class StreamSubscriptionValidator<T> where T : Actor
    {
        public static async Task<bool> AreMatched(
            string fromStream,
            string targetId)
        {
            Core.ActorType.Reset();
            Core.ActorType.Register(new[] { typeof(T).Assembly });
            var system = new ActorSystemMock();

            var actorType = Core.ActorType.From(typeof(T));

            var specs = StreamSubscriptionSpecification.From(actorType);

            var matched =
                specs
                .Select(s=> s.Match(system, fromStream))
                .Where(m => m  != StreamSubscriptionMatch.None)
                .ToArray();
            if (!matched.Any())
                return false;


            var recievedBy = system.MockActorOf<T>(targetId);
            foreach (var match in matched)
            {
                await match.Receiver("");
            }
            return recievedBy.RecordedMessages.Any();
        }
    }

}
