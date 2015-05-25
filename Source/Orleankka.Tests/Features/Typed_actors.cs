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

            public string GetText()
            {
                return TextField;
            }

            public void SetText(string arg)
            {
                TextField = arg;
            }

            public Task SetTextReturnsTask(string arg)
            {
                TextField = arg;
                return TaskDone.Done;
            }

            public Task<string> GetTextReturnsTask()
            {
                return Task.FromResult(TextField);
            }

            public async Task SetTextAsync(string arg)
            {
                var another = System.TypedActorOf<TestActor>("foo");
                await another.Call(x => x.SetText(arg));
            }

            public async Task<string> GetTextAsync()
            {
                var another = System.TypedActorOf<TestActor>("foo");
                return await another.Call(x => x.GetText());
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
            public async void Calling_task_returning_methods()
            {
                var actor = system.FreshTypedActorOf<TestActor>();

                await actor.Call(x => x.SetTextReturnsTask("c-a"));

                Assert.AreEqual("c-a", await actor.Call(x => x.GetTextReturnsTask()));
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