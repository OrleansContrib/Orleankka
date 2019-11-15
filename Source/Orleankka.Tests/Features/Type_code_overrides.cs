using System;
using System.Threading.Tasks;

using NUnit.Framework;

using Orleans.CodeGeneration;
using Orleans.Internals;
using Orleans.Runtime;

namespace Orleankka.Features
{
    namespace Type_code_overrides
    {
        using Meta;
        using Testing;

        [TypeCodeOverride(4241)]
        public interface ITestActor : IActor
        {}

        [TypeCodeOverride(4242)]
        public class TestActor : Actor, ITestActor
        {
            void On(string s) {}
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
            public async Task When_applied_to_interface_and_class()
            {
                var actor = system.FreshActorOf<ITestActor>();
                await actor.Tell("foo");

                var @ref = (GrainReference) actor;
                Assert.That(@ref.InterfaceId, Is.EqualTo(4241));
                Assert.That(@ref.Identity().TypeCode, Is.EqualTo(4242));
            }
        }
    }
}