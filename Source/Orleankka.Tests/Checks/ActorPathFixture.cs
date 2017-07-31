using NUnit.Framework;

namespace Orleankka.Checks
{
    [TestFixture]
    public class ActorPathFixture
    {
        [Test]
        public void Can_be_constructed_and_serialized_without_actor_registration()
        {
            var path = ActorPath.From("T", "42");
            Assert.AreEqual("T:42", path.ToString());
        }
    }
}
