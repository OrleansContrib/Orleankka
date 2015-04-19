using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using NUnit.Framework;
using Orleans;

namespace Orleankka.Features
{
    namespace Using_reminders
    {
        using Meta;
        using Testing;
        using Services;

        [Serializable]
        public class SetReminder : Command
        {
            public TimeSpan Period;
        }

        [Serializable]
        public class HasBeenReminded : Query<bool>
        {}

        [Serializable]
        public class GetInstanceHashcode : Query<int>
        {}

        public class TestActor : Actor
        {
            readonly IReminderService reminders;
            bool reminded;

            protected TestActor()
            {
                reminders = new ReminderService(this);
            }

            protected internal override void Define()
            {
                On((HasBeenReminded x)      => reminded);
                On((SetReminder x)          => reminders.Register("test", TimeSpan.Zero, x.Period));
                On((GetInstanceHashcode x)  => RuntimeHelpers.GetHashCode(this));
            }

            protected internal override Task OnReminder(string id)
            {
                reminded = true;
                return TaskDone.Done;
            }
        }

        [TestFixture]
        [Explicit, Category("Slow")]
        [RequiresSilo(Fresh = true, DefaultKeepAliveTimeoutInMinutes = 1)]
        public class Using_reminders
        {
            IActorSystem system;

            [SetUp]
            public void SetUp()
            {
                system = TestActorSystem.Instance;
            }

            [Test]
            public async void When_reminder_is_fired_an_instance_of_correct_actor_type_should_be_activated()
            {
                var actor = system.FreshActorOf<TestActor>();
                var hashcode = await actor.Ask(new GetInstanceHashcode());

                await actor.Tell(new SetReminder {Period = TimeSpan.FromMinutes(1.5)});
                await Task.Delay(TimeSpan.FromMinutes(2.0));

                Assert.True(await actor.Ask<bool>(new HasBeenReminded()));
                Assert.AreNotEqual(hashcode, await actor.Ask(new GetInstanceHashcode()));
            }
        }
    }
}
