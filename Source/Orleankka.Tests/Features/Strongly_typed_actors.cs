using System;
using System.Threading.Tasks;

using NUnit.Framework;

namespace Orleankka.Features
{
    namespace Strongly_typed_actors
    {
        using Meta;
        using Testing;

        [Serializable] public class TestActorCommand : Command<TestActor> {}
        [Serializable] public class TestActorQuery : Query<TestActor, long> {}

        public class TestActor : Actor
        {
            void On(TestActorCommand x) {}
            long On(TestActorQuery x) => 42;
        }

        [Serializable]
        public class TestAnotherActorCommand : Command<TestAnotherActor>
        {
            public ActorRef<TestActor> Ref;
        }

        public class TestAnotherActor : Actor
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
                var actor = system.TypedActorOf<TestActor>("foo");

                // below won't compile
                // actor.Tell(new object());

                Assert.DoesNotThrow(async ()=> await actor.Tell(new TestActorCommand()));
                Assert.That(await actor.Ask(new TestActorQuery()), Is.EqualTo(42));
            }

            [Test]
            public void Typed_actor_ref_serialization()
            {
                var actor = system.TypedActorOf<TestAnotherActor>("bar");

                var cmd = new TestAnotherActorCommand
                {
                    Ref = system.TypedActorOf<TestActor>("foo")
                };

                Assert.DoesNotThrow(async () => await actor.Tell(cmd));
            }
        }
    }
}