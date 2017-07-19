using System.Reflection;

using NUnit.Framework;

using Orleans.CodeGeneration;

namespace Orleankka.Features
{
    namespace Native_orleans_attributes
    {
        using Core;
        using Testing;

        [Version(22)]
        public interface ITestVersionActor : IActor {}
        public class TestVersionActor : Actor, ITestVersionActor {}

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
            public void Version_attribute()
            {
                var @interface = ActorInterface.Of(ActorTypeName.Of(typeof(TestVersionActor)));
                var attribute = @interface.Grain.GetCustomAttribute<VersionAttribute>();
                Assert.That(attribute, Is.Not.Null);
                Assert.That(attribute.Version, Is.EqualTo(22));
            }
        }
    }
}