using System;
using System.Linq;

using NUnit.Framework;

namespace Orleankka.Scenarios
{
    [TestFixture]
    public class Unwrapping_exceptions
    {
        [Test]
        public void Should_unwrap_exception()
        {
            var system = new ActorSystem();
            var actor  = system.ActorOf<ITestActor>("test");

            Assert.Throws<ApplicationException>(async ()=>
            {
                var message = new Throw {Exception = new ApplicationException("err")};
                await actor.Tell(message);
            });
        }
    }
}