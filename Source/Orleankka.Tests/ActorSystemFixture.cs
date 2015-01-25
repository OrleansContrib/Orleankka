using System;
using System.Linq;

using NUnit.Framework;

namespace Orleankka
{
    [TestFixture]
    public class ActorSystemFixture
    {
        IActorSystem system;

        [SetUp]
        public void SetUp()
        {
            system = new ActorSystem();
        }
        
        [Test]
        public void Should_throw_if_passed_type_is_not_an_interface_which_implements_IActor()
        {
            Assert.Throws<ArgumentException>(() => system.ActorOf<INonActorInterface>("123"));
        }

        [Test]
        public void Should_throw_if_passed_type_is_IActor_interface_itself()
        {
            Assert.Throws<ArgumentException>(() => system.ActorOf<IActor>("123"));
        }

        [Test]
        public void Should_find_closest_IActor_inherited_interface()
        {
            Assert.That(ActorSystem.InterfaceOf(typeof(ActorClass)), 
                Is.SameAs(typeof(IActorFinalInterface)));
        }

        [Test]
        public void But_which_is_not_IActor_itself()
        {
            Assert.Throws<InvalidOperationException>(() => 
                ActorSystem.InterfaceOf(typeof(BadActorClass)));
        }

        interface IActorSubInterface : IActor {}
        interface IActorFinalInterface : IActorSubInterface {}
        interface INonActorInterface {}

        class ActorClass : Actor, IActorFinalInterface, INonActorInterface {}
        class BadActorClass : Actor {}
    }
}
