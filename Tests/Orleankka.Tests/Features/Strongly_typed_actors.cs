using System;
using System.Threading.Tasks;

using NUnit.Framework;

using Orleans;

namespace Orleankka.Features
{
    namespace Strongly_typed_actors
    {
        using Meta;
        using Testing;

        [Serializable]
        public class TestActorCommand : Command<ITestActor> {}

        [Serializable]
        public class TestActorQuery : Query<ITestActor, long> {}

        [Serializable]
        public class TestActorQueryAsync : Query<ITestActor, string> { }

        public interface ITestActor : IActorGrain, IGrainWithStringKey
        {}

        public class TestActor : DispatchActorGrain, ITestActor
        {
            void On(TestActorCommand msg) {}
            long On(TestActorQuery msg) => msg.Response(42);
            async Task<string> On(TestActorQueryAsync msg) => await Task.FromResult(msg.Response("In base 13, what is 6 multiplied by 9?"));
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
            public async Task Request_response_async()
            {
                var actor = system.TypedActorOf<ITestActor>("foo");

                // below won't compile
                // actor.Tell(new object());

                Assert.DoesNotThrowAsync(async () => await actor.Tell(new TestActorCommand()));
                Assert.That(await actor.Ask(new TestActorQueryAsync()), Is.EqualTo("In base 13, what is 6 multiplied by 9?"));
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