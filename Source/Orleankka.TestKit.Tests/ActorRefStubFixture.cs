using System;
using System.Linq;

using NUnit.Framework;

namespace Orleankka.TestKit
{
    [TestFixture]
    public class ActorRefStubFixture
    {
        readonly ActorPath path = new ActorPath(typeof(ITestActor), "some-id");
        ActorRefStub stub;

        [SetUp]
        public void SetUp()
        {
            stub = new ActorRefStub(path);
        }

        [Test]
        public void Remembers_path()
        {
            Assert.That(stub.Path, Is.EqualTo(path));
        } 

        [Test]
        public void Do_nothing_for_commands()
        {
            Assert.DoesNotThrow(async ()=> await stub.Tell(new object()));
        }

        [Test]
        public async void Returns_default_result_for_unmatched_queries()
        {
            Assert.AreEqual(default(int), await stub.Ask<int>(new object()));
            Assert.AreEqual(default(object), await stub.Ask(new object()));
        }

        interface ITestActor : IActor
        {}
    }
}
