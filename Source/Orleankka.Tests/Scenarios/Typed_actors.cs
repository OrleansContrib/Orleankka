using System;
using System.Linq;

using NUnit.Framework;
using Orleankka.Core;

namespace Orleankka.Scenarios
{
    public class TestTypedActor : TypedActor
    {
        string text = "{}";

        protected override void Receive()
        {
            On<SetText>(req => text = req.Text);
            
            On<GetText>(async req =>
            {
                var other = System.ActorOf<AnotherTestTypedActor>("123");
                Reply(await other.Ask(text));
            });
        }
    }

    public class AnotherTestTypedActor : TypedActor
    {
        protected override void Receive()
        {
            On<string>(req => Reply(req));
        }
    }

    [TestFixture]
    public class Function_style_actor
    {
        static readonly IActorSystem system = ActorSystem.Instance;

        [Test]
        public async void Client_to_actor()
        {
            var actor = system.FreshActorOf<TestTypedActor>();

            await actor.Tell(new SetText("c-a"));
            
            Assert.AreEqual("c-a", await actor.Ask(new GetText()));
        }
    }
}
