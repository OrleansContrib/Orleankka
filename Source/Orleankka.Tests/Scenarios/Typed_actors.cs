using System;
using System.Linq;

using NUnit.Framework;

namespace Orleankka.Scenarios
{
    public class TestTypedActor : TypedActor
    {
        string text = "{}";

        protected override void Define()
        {
            On<SetText>(req => text = req.Text);
            
            On<GetText, string>(req =>
            {
                var other = System.ActorOf<AnotherTestTypedActor>("123");
                return other.Ask<string>(text);
            });
        }
    }

    public class AnotherTestTypedActor : TypedActor
    {
        protected override void Define()
        {
            On<string, string>(req => req);
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
            
            Assert.AreEqual("c-a", await actor.Ask<string>(new GetText()));
        }
    }
}
