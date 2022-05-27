using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using NUnit.Framework;

using Orleans;

namespace Orleankka.Features
{
    namespace Using_reminders
    {
        using Meta;
        using Microsoft.CodeAnalysis;
        using Testing;
        using static Syntax;
        
        public record SetReminder(TimeSpan Period) : Command;
        public record Kill : Command;
        public record HasBeenReminded : Query<bool>;
        public record InstanceHashcode : Query<long>;

        public interface ITestActor : IActorGrain, IGrainWithStringKey {}
        public class TestActor : DispatchActorGrain, ITestActor
        {
            bool reminded;

            void On(Reminder _)             => reminded = true;
            bool On(HasBeenReminded x)      => reminded;
            void On(SetReminder x)          => Reminders.Register("test", TimeSpan.Zero, x.Period);
            void On(Kill _)                 => Activation.DeactivateOnIdle();
            long On(InstanceHashcode _)  => this.GetHashCode();
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
                var actor = system.FreshActorOf<ITestActor>();
                var hashcode = await (result(new InstanceHashcode()) > actor);

                await (actor < new SetReminder(TimeSpan.FromMinutes(1.5)));
                await (actor < new Kill());
                await Task.Delay(TimeSpan.FromMinutes(2.0));

                Assert.True(await (result(new HasBeenReminded()) > actor));
                Assert.AreNotEqual(hashcode, await actor.Ask(new InstanceHashcode()));
            }
        }
    }
}
