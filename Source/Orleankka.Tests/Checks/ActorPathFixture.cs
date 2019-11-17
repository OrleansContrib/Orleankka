using NUnit.Framework;

namespace Orleankka.Checks
{
    [TestFixture]
    public class ActorPathFixture
    {
        public interface ITestActor : IActor {} 
        class TestActor : Actor, ITestActor  { }

        [Test]
        public void Can_be_constructed_and_serialized_without_actor_registration()
        {
            var path = ActorPath.For(typeof(ITestActor), "42");
            Assert.AreEqual("Orleankka.Checks.ActorPathFixture+ITestActor:42", path.ToString());
        }
    }
}
