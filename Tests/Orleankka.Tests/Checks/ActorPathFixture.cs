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
            var path = ActorPath.For<ITestActor>("42");
            var path1 = ActorPath.For<TestActor>("42");

            Assert.True(path == path1);
            Assert.True(path.Equals(path1));
        }
    }
}