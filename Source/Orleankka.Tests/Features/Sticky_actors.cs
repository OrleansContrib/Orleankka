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
        public class Activate : Command
        {}

        [Serializable]
        public class Deactivate : Command
        {}

        public interface ITestActor : IActorGrain
        {}

        [Sticky]
        public class TestActor : ActorGrain, ITestActor
        {
            void On(Activate x) {}

            public override async Task OnActivate()
            {
                var stream = System.StreamOf("sms", "sticky");
                await stream.Push("alive!");
            }

            void On(Deactivate q) => Activation.DeactivateOnIdle();
        }

        [TestFixture, RequiresSilo]
        [Category("Slow")]
        public class Tests
        {
            IActorSystem system;

            [SetUp]
            public void SetUp()
            {
                system = TestActorSystem.Instance;
            }

            [Test]
            public async Task Sticky_actors_shoud_be_automatically_resurrected()
            {
                var events = new List<string>();

                var stream = system.StreamOf("sms", "sticky");
                await stream.Subscribe<string>(e => events.Add(e));

                var sticky = system.ActorOf<TestActor>("sticky");
                await sticky.Tell(new Activate());

                // first activation (from Activate message)
                Assert.That(events.Count, Is.EqualTo(1));

                // deactivate
                await sticky.Tell(new Deactivate());

                // wait until reminder timeout (1 minute min)
                await Task.Delay(TimeSpan.FromMinutes(2));

                // auto-reactivation (from automatically registered reminder message)
                Assert.That(events.Count, Is.EqualTo(2));
            }
        }
    }
}