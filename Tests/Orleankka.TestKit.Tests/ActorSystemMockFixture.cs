using System;
using System.Linq;

using NUnit.Framework;

using Orleans;

namespace Orleankka.TestKit
{
    interface ITestActorSystemActor : IActorGrain, IGrainWithStringKey
    {}

    class TestActorSystemActor : DispatchActorGrain, ITestActorSystemActor
    {
        public TestActorSystemActor(string id = null, IActorRuntime runtime = null)
        : base(id, runtime)
        { }

        public void Handle(string part) => System.ActorOf<ITestActorSystemActor>(part + Random.Shared.Next(10));
        public void Handle(int part) => System.StreamOf<int>("sms", $"{part}{Random.Shared.Next(10)}");
    }

    [TestFixture]
    public class ActorSystemMockFixture
    {
        ActorRuntimeMock runtime;
        ActorSystemMock system;

        [SetUp]
        public void SetUpTest()
        {
            runtime = new ActorRuntimeMock();
            system = runtime.System;         
        }

        [Test]
        public void Returns_actor_mock_if_it_was_previosly_set_up()
        {
            var mock = system.MockActorOf<ITestActorSystemActor>("expected-id");
            Assert.AreSame(mock, system.ActorOf<ITestActorSystemActor>("expected-id"));
        }

        [Test]
        public void Returns_new_actor_mock_even_if_no_actor_mock_was_previosly_set_up()
        {
            var mock = system.ActorOf<ITestActorSystemActor>("unexpected-id");
            Assert.IsInstanceOf<ActorRefMock>(mock);
        }

        [Test]
        public void Returns_same_actor_mock_instance_for_unexpected_calls()
        {
            var mock1 = system.ActorOf<ITestActorSystemActor>("unexpected-id");
            var mock2 = system.ActorOf<ITestActorSystemActor>("unexpected-id");

            Assert.IsInstanceOf<ActorRefMock>(mock1);
            Assert.IsInstanceOf<ActorRefMock>(mock2);

            Assert.AreSame(mock1, mock2);
        }
        
        [Test]
        public void Records_actors_and_streams()
        {
            var actor = new TestActorSystemActor("test", runtime);

            actor.Handle("other");
            actor.Handle(1);

            Assert.IsTrue(system.RecordedActors.Count() == 1);
            Assert.IsTrue(system.RecordedStreams<int>().Count() == 1);
        }
    }
}
