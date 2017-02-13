using System;
using System.Threading.Tasks;

using NUnit.Framework;

using Orleans.Providers;

namespace Orleankka.Features
{
    namespace Stateful_actors
    {
        using Meta;
        using Testing;

        [Serializable]
        public class SetState : Command
        {
            public string Data;
        }

        [Serializable]
        public class GetState : Query<string>
        {}

        [StorageProvider(ProviderName = "MemoryStore")]
        public class TestActor : StatefulActor<TestActor.TestState>
        {
            public async Task Handle(SetState cmd)
            {
                State.Data = cmd.Data;
                await WriteState();
            }

            public async Task<string> Handle(GetState query)
            {
                await ReadState();
                return State.Data;
            }

            [Serializable]
            public class TestState
            {
                public string Data;
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
            public async void When_using_default_storage_service()
            {
                var actor = system.FreshActorOf<TestActor>();

                await actor.Tell(new SetState {Data = "foo"});
                Assert.AreEqual("foo", await actor.Ask(new GetState()));
            }
        }
    }
}