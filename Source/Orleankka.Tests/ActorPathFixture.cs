using System;
using System.Linq;

using NUnit.Framework;

namespace Orleankka
{
    [TestFixture]
    public class ActorPathFixture
    {
        [Test]
        public void Should_throw_if_passed_type_is_not_an_interface_which_implements_IActor()
        {
            Assert.Throws<ArgumentException>(() => new ActorPath(typeof(INonActorInterface), "123"));
        }

        [Test]
        public void Should_throw_if_passed_type_is_IActor_interface_itself()
        {
            Assert.Throws<ArgumentException>(() => new ActorPath(typeof(IActor), "123"));
        }

        [Test]
        public void Should_find_closest_IActor_inherited_interface()
        {
            Assert.That(ActorPath.Map(typeof(ActorClass), "123"), 
                Is.EqualTo(new ActorPath(typeof(IActorFinalInterface), "123")));
        }

        [Test]
        public void But_which_is_not_IActor_itself()
        {
            Assert.Throws<InvalidOperationException>(() => ActorPath.Map(typeof(BadActorClass), "123"));
        }

        interface IActorSubInterface : IActor {}
        interface IActorFinalInterface : IActorSubInterface {}
        interface INonActorInterface {}

        class ActorClass : Actor, IActorFinalInterface, INonActorInterface {}
        class BadActorClass : Actor {}
    }
}
