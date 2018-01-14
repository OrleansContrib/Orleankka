using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using NUnit.Framework;

namespace Orleankka.Features
{
    namespace Using_reminders
    {
        using Meta;
        using Testing;

        [Serializable]
        public class SetReminder : Command
        {
            public TimeSpan Period;
        }
        
        [Serializable]
        public class Kill : Command
        {}

        [Serializable]
        public class HasBeenReminded : Query<bool>
        {}

        [Serializable]
        public class GetInstanceHashcode : Query<long>
        {}

        public interface ITestActor : IActorGrain
        {}

        public class TestActor : ActorGrain, ITestActor
        {
            bool reminded;

            void On(Reminder _)             => reminded = true;
            bool On(HasBeenReminded x)      => reminded;
            void On(SetReminder x)          => Reminders.Register("test", TimeSpan.Zero, x.Period);
            void On(Kill x)                 => Activation.DeactivateOnIdle();
            long On(GetInstanceHashcode x)  => RuntimeHelpers.GetHashCode(this);
            
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
            public async Task When_reminder_is_fired_an_instance_of_correct_actor_type_should_be_activated()
            {
                var actor = system.FreshActorOf<TestActor>();
                var hashcode = await actor.Ask(new GetInstanceHashcode());

                await actor.Tell(new SetReminder {Period = TimeSpan.FromMinutes(1.5)});
                await actor.Tell(new Kill());
                await Task.Delay(TimeSpan.FromMinutes(2.0));

                Assert.True(await actor.Ask<bool>(new HasBeenReminded()));
                Assert.AreNotEqual(hashcode, await actor.Ask(new GetInstanceHashcode()));
            }
        }
    }
}
