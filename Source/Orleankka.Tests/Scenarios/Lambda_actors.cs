using System;
using System.Linq;

using NUnit.Framework;

namespace Orleankka.Scenarios
{
    using Meta;
    using Testing;

    [RequiresSilo]
    public class Lambda_actors : ActorSystemScenario
    {
        [Test]
        public async void Handlers_could_be_defined_via_prototype()
        {
            var actor = system.FreshActorOf<TestLambdaActor>();
            
            await actor.Tell(new SetText("c-a"));
            
            Assert.AreEqual("c-a", await actor.Ask(new GetText()));
        }

        class TestLambdaActor : Actor
        {
            string text = "{}";

            protected internal override void Define()
            {
                On((SetText x) => text = x.Text);

                On((GetText x) =>
                {
                    var other = System.ActorOf<AnotherTestLambdaActor>("123");
                    return other.Ask<string>(text);
                });
            }
        }

        class AnotherTestLambdaActor : Actor
        {
            protected internal override void Define()
            {
                On<string, string>(x => x);
            }
        }
    }
}
