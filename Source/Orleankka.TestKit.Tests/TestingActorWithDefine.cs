using NUnit.Framework;
using Orleankka.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orleankka.TestKit
{
    [TestFixture]
    public class TestingActorWithDefine
    {
        [TestCase]
        public async Task Actor_with_define_can_be_unit_tested()
        {
            var actor = new TestActor();
            actor.Define();
            await actor.OnReceive(new Handled());
            Assert.IsTrue(actor.Handled);
        }

        [TestCase]
        public async Task Actor_can_be_tested_if_OnRecieve_is_overriden()
        {
            var actor = new ActorForbiddingEventRecieves();
            actor.Define();
            Assert.Throws<InvalidOperationException>(async ()=>
                await actor.OnReceive(new Handled())
            );
            await actor.Dispatch(new Handled());
            Assert.IsTrue(actor.Handled);
        }
    }


    public class ActorForbiddingEventRecieves : Actor
    {
        protected override Task<object> OnReceive(object message)
        {
            if(message is Event)
                throw new InvalidOperationException("I pretend to be EventSourced and don't accept Events");
            return Task.FromResult(message);
        }
        public bool Handled { get; private set; }
        protected override void Define()
        {
            On<Handled>(ev => Handled = true);
        }
    }

    public class TestActor : Actor {

        public bool Handled { get; private set; }
        protected override void Define()
        {
            On<Handled>(ev => Handled = true);
        }
    }

    public class Handled:Event{}
}
