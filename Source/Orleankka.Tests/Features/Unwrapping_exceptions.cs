using System;
using System.Linq;
using System.Threading.Tasks;

using NUnit.Framework;

namespace Orleankka.Features
{
    namespace Unwrapping_exceptions
    {
        using Meta;
        using Testing;

        [Serializable]
        public class Throw : Command
        {
            public Exception Exception;
        }

        public class TestActor : Actor
        {
            public void Handle(Throw cmd)
            {
                throw cmd.Exception;
            }
        }

        [Serializable]
        public class DoTell : Command
        {
            public ActorRef Target;
            public object Message;
        }

        public class TestInsideActor : Actor
        {
            public async Task Handle(DoTell cmd)
            {
                await cmd.Target.Tell(cmd.Message);
            }
        }

        [TestFixture]
        [RequiresSilo]
        public class Tests
        {
            IActorSystem system;

            [SetUp]
            public void SetUp()
            {
                system = TestActorSystem.Instance;
            }

            [Test]
            public void Client_to_actor()
            {
                var actor = system.FreshActorOf<TestActor>();

                Assert.Throws<ApplicationException>(async () => await 
                    actor.Tell(new Throw {Exception = new ApplicationException("c-a")}));
            }

            [Test]
            public void Actor_to_actor()
            {
                var one = system.FreshActorOf<TestInsideActor>();
                var another = system.FreshActorOf<TestActor>();

                Assert.Throws<ApplicationException>(async () =>
                {
                    var message = new Throw {Exception = new ApplicationException("a-a")};
                    await one.Tell(new DoTell {Target = another, Message = message});
                });
            }
        }
    }
}