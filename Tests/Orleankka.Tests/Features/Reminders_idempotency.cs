using System;
using System.Threading.Tasks;

using NUnit.Framework;

using Orleans;

namespace Orleankka.Features
{
    namespace Reminders_idempotency
    {
        using Meta;
        using Testing;

        [Serializable]
        public class RegisterReminder : Command
        {
            public string Name;
        }

        [Serializable]
        public class UnregisterReminder : Command
        {
            public string Name;
        }

        [Serializable]
        public class IsReminderRegistered : Query<bool>
        {
            public string Name;
        }

        public interface ITestActor : IActorGrain, IGrainWithStringKey
        {}

        public class TestActor : DispatchActorGrain, ITestActor
        {
            Task On(RegisterReminder x) => Reminders.Register(x.Name, TimeSpan.FromHours(10), TimeSpan.FromHours(10));
            Task On(UnregisterReminder x) => Reminders.Unregister(x.Name);
            Task<bool> On(IsReminderRegistered x) => Reminders.IsRegistered(x.Name);
        }

        [TestFixture]
        [RequiresSilo]
        public class Tests
        {
            IActorSystem system;

            [SetUp]
            public void SetUp() => system = TestActorSystem.Instance;

            [Test]
            public async Task When_unregistering_never_registered()
            {
                var actor = system.FreshActorOf<ITestActor>();
                Assert.DoesNotThrowAsync(async ()=> await actor.Tell(new UnregisterReminder {Name = "unknown"}));

                var another = system.FreshActorOf<ITestActor>();
                Assert.False(await another.Ask(new IsReminderRegistered {Name = "unknown"}));
                Assert.DoesNotThrowAsync(async ()=> await another.Tell(new UnregisterReminder {Name = "unknown"}));
            }

            [Test]
            public async Task When_unregistering_twice()
            {
                var actor = system.FreshActorOf<ITestActor>();

                await actor.Tell(new RegisterReminder {Name = "test"});
                await actor.Tell(new UnregisterReminder {Name = "test"});
                Assert.DoesNotThrowAsync(async ()=> await actor.Tell(new UnregisterReminder {Name = "test"}));
            }

            [Test]
            [Description("This tests case with repeated Unregister in case of repeated forwarding (MaxForwardCount=2 by default")]
            public async Task When_unregistering_deleted_reminder()
            {
                var actor = system.FreshActorOf<ITestActor>();
                
                await actor.Tell(new RegisterReminder {Name = "deleted"});
                Assert.True(await actor.Ask(new IsReminderRegistered { Name = "deleted" }));
                
                await CleanRemindersTable();

                Assert.DoesNotThrowAsync(async () => await actor.Tell(new UnregisterReminder {Name = "deleted" }));
                Assert.False(await actor.Ask(new IsReminderRegistered { Name = "deleted" }));
            }

            static async Task CleanRemindersTable()
            {
                var getGrainGeneric = typeof(IGrainFactory).GetMethod("GetGrain", new[]{typeof(long), typeof(string)});
                var getGrain = getGrainGeneric.MakeGenericMethod(typeof(IReminderTable).Assembly.GetType("Orleans.IReminderTableGrain"));
                var grain = getGrain.Invoke(TestActorSystem.Client, new object[]{12345, null});
                await (Task) grain.GetType().GetMethod("TestOnlyClearTable").Invoke(grain, new object[0]);
            }
        }
    }
}
