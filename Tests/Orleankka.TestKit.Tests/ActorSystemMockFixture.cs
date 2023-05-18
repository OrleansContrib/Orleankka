using NUnit.Framework;

using Orleans;

namespace Orleankka.TestKit
{
    interface ITestActorSystemActor : IActorGrain, IGrainWithStringKey
    {}

    class TestActorSystemActor : DispatchActorGrain, ITestActorSystemActor
    {}

    [TestFixture]
    public class ActorSystemMockFixture
    {
        ActorSystemMock system;

        [SetUp]
        public void SetUpTest()
        {
            system = new ActorSystemMock();           
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
    }
}
