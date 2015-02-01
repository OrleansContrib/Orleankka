using System;
using System.Linq;

using NUnit.Framework;

namespace Orleankka.Fixtures
{
    [TestFixture]
    public class ActorRefFixture
    {
        [Test]
        public void Equatable_by_path()
        {
            var path = ActorPath.From("a::42");

            var ref1 = new ActorRef(path);
            var ref2 = new ActorRef(path);
            
            Assert.True(ref1 == ref2);
            Assert.True(ref1.Equals(ref2));
        }
    }
}
