using System;
using System.Threading.Tasks;

using NUnit.Framework;

using Orleankka.Meta;

namespace Orleankka.Features
{
    namespace Strongly_typed_actors
    {
        using Testing;

        [Serializable]
        public class TestActorCommand : Command<ITestActor> {}

        [Serializable]
        public class TestActorQuery : Query<ITestActor, long> {}

        public interface ITestActor : IActorGrain
        {}

        public class TestActor : ActorGrain, ITestActor
        {
            void On(TestActorCommand x) {}
            long On(TestActorQuery x) => 42;
        }

        [Serializable]
        public class TestAnotherActorCommand : Command<ITestAnotherActor>
        {
            public ActorRef<ITestActor> Ref;
        }

        public interface ITestAnotherActor : IActorGrain
        {}

        public class TestAnotherActor : ActorGrain, ITestAnotherActor
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
            public async Task Request_response()
            {
                var actor = system.TypedActorOf<ITestActor>("foo");

                // below won't compile
                // actor.Tell(new object());

                Assert.DoesNotThrowAsync(async () => await actor.Tell(new TestActorCommand()));
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

                Assert.DoesNotThrowAsync(async () => await actor.Tell(cmd));
            }
        }
    }
}