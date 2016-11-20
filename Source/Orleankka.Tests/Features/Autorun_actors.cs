using System;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

namespace Orleankka.Features
{
    namespace Autorun_actors
    {
        using Meta;
        using Testing;

        [Serializable]
        public class WhenActivated : Query<DateTime>
        {}

        [Autorun("a1")]
        [Autorun("a2")]
        public class TestActor : Actor
        {
            DateTime activated;

            public override Task OnActivate()
            {
                activated = DateTime.Now;
                return base.OnActivate();
            }

            DateTime On(WhenActivated q) => activated;
        }

        [TestFixture]
        [RequiresSilo(Fresh = true)]
        [Explicit, Category("Slow")]
        public class Tests
        {
            IActorSystem system;

            [SetUp]
            public void SetUp()
            {
                system = TestActorSystem.Instance;
            }

            [Test]
            public async void Autorun_actors_shoud_be_automatically_activated_during_boot()
            {
                // wait a bit
                Thread.Sleep(TimeSpan.FromSeconds(5));

                var a1 = system.ActorOf<TestActor>("a1");
                var a2 = system.ActorOf<TestActor>("a1");

                Assert.That(await a1.Ask(new WhenActivated()), Is.LessThan(DateTime.Now.AddSeconds(-2)));
                Assert.That(await a2.Ask(new WhenActivated()), Is.LessThan(DateTime.Now.AddSeconds(-2)));
            }
        }
    }
}