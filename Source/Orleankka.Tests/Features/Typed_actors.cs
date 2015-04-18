using System;
using System.Linq;

using NUnit.Framework;

namespace Orleankka.Features
{
    namespace Typed_actors
    {
        using Typed;
        using Testing;

        public class TestActor : TypedActor
        {
            string text = "";

            public void SetText(string arg)
            {
                text = arg;
            }

            public string GetText()
            {
                return text;
            }
        }

        [TestFixture]
        [RequiresSilo]
        public class Typed_actors
        {
            IActorSystem system;

            [SetUp]
            public void SetUp()
            {
                system = TestActorSystem.Instance;
            }

            [Test]
            public async void Calling_non_async_methods()
            {
                var actor = system.FreshTypedActorOf<TestActor>();

                await actor.Call(x => x.SetText("c-a"));
                
                Assert.AreEqual("c-a", await actor.Call(x => x.GetText()));
            }
        }
    }
}