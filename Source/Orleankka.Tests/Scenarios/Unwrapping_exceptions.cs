using System;
using System.Linq;

using NUnit.Framework;

namespace Orleankka.Scenarios
{
    [TestFixture]
    public class Unwrapping_exceptions
    {
        static readonly IActorSystem system = ActorSystem.Instance;

        [Test]
        public void Client_to_actor()
        {
            var actor = system.FreshActorOf<ITestActor>();

            Assert.Throws<ApplicationException>(async ()=> await 
                actor.Tell(new Throw(new ApplicationException("c-a"))));
        }

        [Test]
        public void Actor_to_actor()
        {
            var one = system.FreshActorOf<ITestInsideActor>();
            var another = system.FreshActorOf<ITestActor>();

            Assert.Throws<ApplicationException>(async ()=> await 
                one.Tell(new DoTell(another, new Throw(new ApplicationException("a-a")))));
        }
    }
}