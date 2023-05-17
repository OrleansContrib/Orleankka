using System;
using System.Threading.Tasks;

using NUnit.Framework;

using Orleans;

namespace Orleankka.Features
{
    namespace Strongly_typed_actors
    {
        using Meta;
        using Orleans.Metadata;

        using Testing;
        using static Syntax;

        [Serializable]
        public class TestActorCommand : Command<ITestActor> {}

        [Serializable]
        public class TestActorQuery : Query<ITestActor, long> {}

        [DefaultGrainType("strong-test")]
        public interface ITestActor : IActorGrain, IGrainWithStringKey
        {}

        [GrainType("strong-test")]
        public class TestActor : DispatchActorGrain, ITestActor
        {
            void On(TestActorCommand msg) {}
            long On(TestActorQuery msg) => msg.Result(42);
        }

        [Serializable]
        public class TestAnotherActorCommand : Command<ITestAnotherActor>
        {
            public ActorRef<ITestActor> Ref;
        }

        public interface ITestAnotherActor : IActorGrain, IGrainWithStringKey
        {}

        public class TestAnotherActor : DispatchActorGrain, ITestAnotherActor
        {
            Task On(TestAnotherActorCommand x) => x.Ref < new TestActorCommand();
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

                Assert.DoesNotThrowAsync(async () => await (actor < new TestActorCommand()));
                Assert.That(await (result(new TestActorQuery()) > actor), Is.EqualTo(42));
            }

            [Test]
            public void Typed_actor_ref_serialization()
            {
                var actor = system.TypedActorOf<ITestAnotherActor>("bar");

                var cmd = new TestAnotherActorCommand
                {
                    Ref = system.TypedActorOf<ITestActor>("foo")
                };

                Assert.DoesNotThrowAsync(async () => await (actor < cmd));
            }
        }
    }
}