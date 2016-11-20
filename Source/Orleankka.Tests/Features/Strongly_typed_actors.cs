using System;
using System.Threading.Tasks;

using NUnit.Framework;

namespace Orleankka.Features
{
    namespace Strongly_typed_actors
    {
        using Meta.CSharp;
        using Testing;

        [Serializable]
        public class TestActorCommand : Command<ITestActor> {}

        [Serializable]
        public class TestActorQuery : Query<ITestActor, long> {}

        public interface ITestActor : IActor
        {}

        public class TestActor : Actor, ITestActor
        {
            void On(TestActorCommand x) {}
            long On(TestActorQuery x) => 42;
        }

        [Serializable]
        public class TestAnotherActorCommand : Command<ITestAnotherActor>
        {
            public ActorRef<ITestActor> Ref;
        }

        public interface ITestAnotherActor : IActor
        {}

        public class TestAnotherActor : Actor, ITestAnotherActor
        {
            Task On(TestAnotherActorCommand x) => x.Ref.Tell(new TestActorCommand());
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
            public async void Request_response()
            {
                var actor = system.TypedActorOf<ITestActor>("foo");

                // below won't compile
                // actor.Tell(new object());

                Assert.DoesNotThrow(async () => await actor.Tell(new TestActorCommand()));
                Assert.That(await actor.Ask(new TestActorQuery()), Is.EqualTo(42));
            }

            [Test]
            public void Typed_actor_ref_serialization()
            {
                var actor = system.TypedActorOf<ITestAnotherActor>("bar");

                var cmd = new TestAnotherActorCommand
                {
                    Ref = system.TypedActorOf<ITestActor>("foo")
                };

                Assert.DoesNotThrow(async () => await actor.Tell(cmd));
            }
        }
    }
}