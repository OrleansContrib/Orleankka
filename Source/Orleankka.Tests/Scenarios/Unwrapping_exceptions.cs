using System;
using System.Linq;

using NUnit.Framework;

namespace Orleankka.Scenarios
{
    using Testing;

    [RequiresSilo]
    public class Unwrapping_exceptions : ActorSystemScenario
    {
        [Test]
        public void Client_to_actor()
        {
            var actor = system.FreshActorOf<TestActor>();

            Assert.Throws<ApplicationException>(async ()=> await 
                actor.Tell(new Throw(new ApplicationException("c-a"))));
        }

        [Test]
        public void Actor_to_actor()
        {
            var one = system.FreshActorOf<TestInsideActor>();
            var another = system.FreshActorOf<TestActor>();

            Assert.Throws<ApplicationException>(async ()=> await 
                one.Tell(new DoTell(another, new Throw(new ApplicationException("a-a")))));
        }
    }
}