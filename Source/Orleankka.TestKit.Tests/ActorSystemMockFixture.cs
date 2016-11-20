using NUnit.Framework;

namespace Orleankka.TestKit
{
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
            var mock = system.MockActorOf<TestActor>("expected-id");
            Assert.AreSame(mock, system.ActorOf<TestActor>("expected-id"));
        }

        [Test]
        public void Returns_new_actor_mock_even_if_no_actor_mock_was_previosly_set_up()
        {
            var mock = system.ActorOf<TestActor>("unexpected-id");
            Assert.IsInstanceOf<ActorRefMock>(mock);
        }

        [Test]
        public void Returns_same_actor_mock_instance_for_unexpected_calls()
        {
            var mock1 = system.ActorOf<TestActor>("unexpected-id");
            var mock2 = system.ActorOf<TestActor>("unexpected-id");

            Assert.IsInstanceOf<ActorRefMock>(mock1);
            Assert.IsInstanceOf<ActorRefMock>(mock2);

            Assert.AreSame(mock1, mock2);
        }

        class TestActor : Actor
        {}
    }
}
