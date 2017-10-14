using System;
using System.Threading.Tasks;

using NUnit.Framework;

using Orleans.Internals;
using Orleans.Runtime;

namespace Orleankka.Features
{
    namespace System_introspection
    {
        using Core;
        using Meta;
        using Testing;

        [Serializable]
        public class CheckTypeCodeResolution : Query<string>
        {}

        public interface ITestActor : IActor
        {}

        public class TestActor : Actor, ITestActor
        {
            string On(CheckTypeCodeResolution x)
            {
                GrainReference reference = Self;
                var identity = reference.Identity();
                var typeCode = identity.TypeCode;
                var @interface = ActorType.Of(typeCode);
                return @interface.Name;
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
            public async Task Client_to_actor()
            {
                var actor = system.FreshActorOf<TestActor>();
                var @interface = await actor.Ask(new CheckTypeCodeResolution());
                Assert.That(@interface, Is.EqualTo("Orleankka.Features.System_introspection.ITestActor"));
            }
        }
    }
}