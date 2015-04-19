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
            public string TextField = "";

            public string TextProperty
            {
                get; set;
            }

            public void SetText(string arg)
            {
                TextField = arg;
            }

            public string GetText()
            {
                return TextField;
            }

            public Task SetTextAsync(string arg)
            {
                TextField = arg;
                return TaskDone.Done;
            }

            public Task<string> GetTextAsync()
            {
                return Task.FromResult(TextField);
            }

            public void SetText(string arg1, string arg2)
            {
                TextField = arg1 ?? arg2;
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
            public async void Calling_overloaded_methods()
            {
                var actor = system.FreshTypedActorOf<TestActor>();

                await actor.Call(x => x.SetText(null, "foo"));

                Assert.AreEqual("foo", await actor.Call(x => x.GetText()));
            }

            [Test]
            public async void Calling_properties()
            {
                var actor = system.FreshTypedActorOf<TestActor>();

                await actor.Set(x => x.TextProperty, "c-a");

                Assert.AreEqual("c-a", await actor.Get(x => x.TextProperty));
            }

            [Test]
            public async void Calling_fields()
            {
                var actor = system.FreshTypedActorOf<TestActor>();

                await actor.Set(x => x.TextField, "foo");

                Assert.AreEqual("foo", await actor.Get(x => x.TextField));
            }
        }
    }
}