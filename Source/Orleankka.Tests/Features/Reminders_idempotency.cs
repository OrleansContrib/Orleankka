using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

using NUnit.Framework;

namespace Orleankka.Features
{
    namespace Reminders_idempotency
    {
        using Meta;
        using Testing;

        [Serializable]
        public class Deactivate : Command {}

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

        public interface ITestActor : IActorGrain
        {}

        public class TestActor : ActorGrain, ITestActor
        {
            Task On(RegisterReminder x) => Reminders.Register(x.Name, TimeSpan.FromHours(10), TimeSpan.FromHours(10));
            Task On(UnregisterReminder x) => Reminders.Unregister(x.Name);
            Task<bool> On(IsReminderRegistered x) => Reminders.IsRegistered(x.Name);
            void On(Deactivate x) => Activation.DeactivateOnIdle();
        }

        [TestFixture]
        [RequiresSilo]
        public class Tests
        {
            IActorSystem system;

            [SetUp]
            public void SetUp()
            {
                system = TestActorSystem.Instance;
                CleanRemindersTable();
            }

            [Test]
            public async Task When_unregistering_never_registered()
            {
                var actor = system.FreshActorOf<TestActor>();
                Assert.DoesNotThrowAsync(async ()=> await actor.Tell(new UnregisterReminder {Name = "unknown"}));

                var another = system.FreshActorOf<TestActor>();
                Assert.False(await another.Ask(new IsReminderRegistered {Name = "unknown"}));
                Assert.DoesNotThrowAsync(async ()=> await another.Tell(new UnregisterReminder {Name = "unknown"}));
            }

            [Test]
            public async Task When_unregistering_twice()
            {
                var actor = system.FreshActorOf<TestActor>();

                await actor.Tell(new RegisterReminder {Name = "test"});
                await actor.Tell(new UnregisterReminder {Name = "test"});
                Assert.DoesNotThrowAsync(async ()=> await actor.Tell(new UnregisterReminder {Name = "test"}));
            }

            [Test]
            [Description("This tests case with repeated Unregister in case of repeated forwarding (MaxForwardCount=2 by default")]
            public async Task When_unregistering_deleted_reminder()
            {
                var actor = system.FreshActorOf<TestActor>();
                
                await actor.Tell(new RegisterReminder {Name = "deleted"});
                Assert.True(await actor.Ask(new IsReminderRegistered { Name = "deleted" }));
                
                CleanRemindersTable();

                Assert.DoesNotThrowAsync(async () => await actor.Tell(new UnregisterReminder {Name = "deleted" }));
                Assert.False(await actor.Ask(new IsReminderRegistered { Name = "deleted" }));
            }

            static void CleanRemindersTable()
            {
                var reminders = CloudStorageAccount.DevelopmentStorageAccount.CreateCloudTableClient().GetTableReference("OrleansReminders");
                var rows = reminders.ExecuteQuerySegmentedAsync(new TableQuery<DynamicTableEntity>(), null).Result;
                rows.Results.ForEach(x => reminders.ExecuteAsync(TableOperation.Delete(x)).Wait());
            }
        }
    }
}
