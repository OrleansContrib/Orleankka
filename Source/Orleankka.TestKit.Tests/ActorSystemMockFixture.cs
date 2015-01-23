using System;
using System.Linq;

using NUnit.Framework;

namespace Orleankka.TestKit
{
    [TestFixture]
    public class ActorSystemMockFixture
    {
        ActorSystemMock system;

        [SetUp]
        public void SetUp()
        {
            system = new ActorSystemMock();
        }

        [Test]
        public void Returns_actor_mock_if_it_was_previosly_set_up()
        {
            var mock = system.MockActorOf<ITestActor>("expected-id");
            Assert.AreSame(mock, system.ActorOf<ITestActor>("expected-id"));
        }

        [Test]
        public void Returns_actor_stub_when_no_actor_mock_was_previosly_set_up()
        {
            var stub = system.ActorOf<ITestActor>("unexpected-id");

            Assert.NotNull(stub);
            Assert.IsInstanceOf<ActorRefStub>(stub);
        }
        
        interface ITestActor : IActor
        {}
    }
}
