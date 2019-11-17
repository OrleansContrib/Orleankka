using NUnit.Framework;

namespace Orleankka.Checks
{
    [TestFixture]
    public class ActorRefFixture
    {
        public interface ITestActor : IActor {} 
        class TestActor : Actor, ITestActor  { }

        [Test]
        public void Equatable_by_path()
        {
            var path = ActorPath.For(typeof(ITestActor), "42");

            var ref1 = new ActorRef(path, null, null);
            var ref2 = new ActorRef(path, null, null);
            
            Assert.True(ref1 == ref2);
            Assert.True(ref1.Equals(ref2));
        }
    }
}
