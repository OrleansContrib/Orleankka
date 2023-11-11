using System;
using System.Threading.Tasks;

using Orleans.Concurrency;

namespace Orleankka.Features
{
    namespace Actor_identity
    {
        using Meta;
        using NUnit.Framework;

        using Orleans;
        using Orleans.Metadata;

        using Testing;

        public record GetPath : Query<ActorPath>;
        public record GetSelfPath : Query<ActorPath>;

        [DefaultGrainType("identity-test")]
        public interface ITestActor : IActorGrain, IGrainWithStringKey { }

        [Reentrant]
        [GrainType("identity-test")]
        public class TestActor : DispatchActorGrain, ITestActor
        {
            public ActorPath On(GetPath q) => Path;
            public Task<ActorPath> On(GetSelfPath q) => Self.Ask<ActorPath>(new GetPath());
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
            public async Task Client_to_actor()
            {
                var actor = system.FreshActorOf<ITestActor>();

                Assert.AreEqual(actor.Path, await actor.Ask(new GetPath()));
            }

            [Test]
            public async Task Actor_to_actor()
            {
                var actor = system.FreshActorOf<ITestActor>();

                Assert.AreEqual(actor.Path, await actor.Ask(new GetSelfPath()));
            }
        }
    }
}
