using System;
using System.Collections.Generic;
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

        public interface ITestActor : IActor
        { }

        public class TestActor : Actor, ITestActor
        {
            const string StickyReminderName = "##sticky##";

            async Task HandleStickyness()
            {
                var period = TimeSpan.FromMinutes(1);
                await Reminders.Register(StickyReminderName, period, period);
            }

            public override async Task OnReminder(string id)
            {
                if (id == StickyReminderName)
                    return;

                await base.OnReminder(id);
            }

            void On(Activate x) {}

            public override async Task OnActivate()
            {
                await HandleStickyness();

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

                var sticky = system.ActorOf<ITestActor>("sticky");
                await sticky.Tell(new Activate());

                await Task.Delay(100);

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