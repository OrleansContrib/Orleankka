using System;
using System.Collections;
using System.Linq;

using NUnit.Framework;

namespace Orleankka
{
    [TestFixture]
    public class ActorSystemFixture
    {
        readonly IActorSystem system = ActorSystem.Instance;

        [Test]
        public void Should_throw_if_passed_type_is_not_an_interface_which_implements_IActor()
        {
            Assert.Throws<ArgumentException>(() => system.ActorOf<IEnumerable>("123"));
        }

        [Test]
        public void Should_throw_if_passed_type_is_IActor_interface_itself()
        {
            Assert.Throws<ArgumentException>(() => system.ActorOf<IActor>("123"));
        }
    }
}
