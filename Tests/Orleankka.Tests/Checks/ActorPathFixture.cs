using NUnit.Framework;

using Orleankka.Testing;

namespace Orleankka.Checks
{
    [TestFixture]
    public class ActorPathFixture
    {
        [Test]
        public void ActorPath_from_class_and_interface_equal()
        {
            var interfacePath = ActorPath.For<ITestActor>("42");
            var classPath = ActorPath.For<TestActor>("42");

            Assert.AreEqual(interfacePath, classPath);
        }
    }
}