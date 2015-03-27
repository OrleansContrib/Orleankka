using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using NUnit.Framework;

namespace Orleankka.Scenarios
{
    using Testing;

    [RequiresSilo(Fresh = true, GCTimeoutInMinutes = 1)]
    public class Automatic_deactivation : ActorSystemScenario
    {
        [Test, Explicit]
        public async void When_no_requests_being_made()
        {
            var actor = system.FreshActorOf<TestAutomaticDeactivationActor>();
            var hashcode = await actor.Ask<long>(new GetInstanceHashcode());

            await Task.Delay(TimeSpan.FromMinutes(2.0));

            Assert.AreEqual(hashcode, await actor.Ask<long>(new GetInstanceHashcode()),
                "Should respect idle timeout defined via the attribute and not being GC'ed as per global timeout");
        }

        [AutomaticDeactivation(Idle = "5m")]
        class TestAutomaticDeactivationActor : Actor
        {
            public int Answer(GetInstanceHashcode q)
            {
                return RuntimeHelpers.GetHashCode(this);
            }
        }
    }
}