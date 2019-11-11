using System;
using System.Linq;

using NUnit.Framework;

namespace Orleankka.Features
{
    using Testing;

    [TestFixture]
    [RequiresSilo]
    public class Custom_type_names
    {
        IActorSystem system;

        [SetUp]
        public void SetUp()
        {
            system = TestActorSystem.Instance;
        }

        [Test]
        public void Type_name_as_default_could_be_overwritten_with_ActorType_attribute()
        {
            var actor1 = system.ActorOf<TypeCode1.ITestActor>("id");
            var actor2 = system.ActorOf<TypeCode2.ITestActor>("id");

            Assert.That(actor1.Path, Is.EqualTo(ActorPath.Parse("T1:id")));
            Assert.That(actor2.Path, Is.EqualTo(ActorPath.Parse("T2:id")));
        }
    }

    namespace TypeCode1
    {
        [ActorType("T1")]
        interface ITestActor : IActor
        { }

        class TestActor : Actor, ITestActor
        {}
    }

    namespace TypeCode2
    {
        [ActorType("T2")]
        interface ITestActor : IActor
        { }

        class TestActor : Actor, ITestActor
        {}
    }
}