
using Orleankka.Core.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orleankka.TestKit
{
    public static class StreamSubscriptionValidator
    {
        public static async Task<bool> MatchesIdOf<T>(
            this string fromStream,
            string targetId,
            object messageToRecieve) where T:Actor
        {
            Core.ActorType.Reset();
            Core.ActorType.Register(new[] { typeof(T).Assembly });
            var system = new ActorSystemMock();

            var actorType = Core.ActorType.From(typeof(T));

            var specs = StreamSubscriptionSpecification.From(actorType);
            var spec = specs.ElementAt(0);
            var match = spec.Match(system, fromStream);
            if (match == StreamSubscriptionMatch.None)
                return false;
            //if (match.Filter(messageToRecieve))
            //    return false;
            var recievedBy = system.MockActorOf<T>(targetId);
            await match.Receiver(messageToRecieve);

            return recievedBy.RecordedMessages.Any();

        }


    }
    
}
