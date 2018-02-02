using System.Threading.Tasks;

using NUnit.Framework;

namespace Orleankka.Checks
{
    using Core;

    [TestFixture]
    public class ActorTypeNameFixture
    {
        public interface ITestActor : IActorGrain
        {}

        public class TestActor : ActorGrain, ITestActor
        {
            public override Task<object> Receive(object message) => throw new System.NotImplementedException();
        }

        [Test]
        public void Can_find_name_from_interface()
        {
            Assert.That(ActorTypeName.Of(typeof(ITestActor)), Is.EqualTo(typeof(ITestActor).FullName));
        } 

        [Test]
        public void Can_find_name_from_type()
        {
            Assert.That(ActorTypeName.Of(typeof(TestActor)), Is.EqualTo(typeof(ITestActor).FullName));
        } 
    }
}