using System;
using System.Linq;

using NUnit.Framework;

namespace Orleankka
{
    [TestFixture]
    public class ActorInterfaceFixture
    {
        [Test]
        public void Should_find_closest_IActor_inherited_interface()
        {
            Assert.That(Actor.Interface.Of(typeof(ActorClass)),  Is.SameAs(typeof(IActorFinalInterface)));
        }

        [Test]
        public void But_which_is_not_IActor_itself()
        {
            Assert.Throws<InvalidOperationException>(() => Actor.Interface.Of(typeof(BadActorClass)));
        }

        interface IActorSubInterface : IActor {}
        interface IActorFinalInterface : IActorSubInterface {}
        interface INonActorInterface {}

        class ActorClass : Actor, IActorFinalInterface, INonActorInterface {}
        class BadActorClass : Actor {}
    }
}
