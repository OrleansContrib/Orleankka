using System;
using System.Linq;
using System.Threading.Tasks;

using NUnit.Framework;
using Orleans;

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

            public Task SetTextAsync(string arg)
            {
                text = arg;
                return TaskDone.Done;
            }

            public Task<string> GetTextAsync()
            {
                return Task.FromResult(text);
            }

            public string TextProperty
            {
                get; set;
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

            [Test]
            public async void Calling_async_methods()
            {
                var actor = system.FreshTypedActorOf<TestActor>();

                await actor.Call(x => x.SetTextAsync("c-a"));

                Assert.AreEqual("c-a", await actor.Call(x => x.GetTextAsync()));
            }

            [Test]
            public async void Calling_properties()
            {
                var actor = system.FreshTypedActorOf<TestActor>();

                await actor.Set(x => x.TextProperty, "c-a");

                Assert.AreEqual("c-a", await actor.Get(x => x.TextProperty));
            }
        }
    }
}