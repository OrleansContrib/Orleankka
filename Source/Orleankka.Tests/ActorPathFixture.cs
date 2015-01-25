using System;
using System.Linq;

using NUnit.Framework;

namespace Orleankka
{
    [TestFixture]
    public class ActorPathFixture
    {
        [Test]
        public void Should_validate_arguments()
        {
            Assert.Throws<ArgumentNullException>(() => new ActorPath(null, "123"));
            Assert.Throws<ArgumentNullException>(() => new ActorPath(GetType(), null));
            Assert.Throws<ArgumentException>(() => new ActorPath(GetType(), ""));
            Assert.Throws<ArgumentException>(() => new ActorPath(GetType(), "  "));
        }
    }
}
