using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

namespace Orleankka.Features
{
    namespace Sticky_actors
    {
        using Meta;
        using Testing;

        [Serializable]
        public class Deactivate : Command
        {}

        [Sticky]
        public class TestActor : Actor
        {
            public override Task OnActivate()
            {
                var stream = System.StreamOf("sms", "sticky");
                return stream.Push("alive!");
            }

            void On(Deactivate q) => Activation.DeactivateOnIdle();
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
            public async void Sticky_actors_shoud_be_automatically_resurrected()
            {
                var events = new List<string>();

                var stream = system.StreamOf("sms", "sticky");
                await stream.Subscribe<string>(e => events.Add(e));

                var sticky = system.ActorOf<TestActor>("sticky");
                await sticky.Tell(new Deactivate());

                // first activation (from Deactivate message)
                Assert.That(events.Count, Is.EqualTo(1));

                // wait until min reminder timeout (1 minute)
                Thread.Sleep(TimeSpan.FromMinutes(1.5));

                // auto-reactivation (from automatically registered reminder message)
                Assert.That(events.Count, Is.EqualTo(2));
            }
        }
    }
}