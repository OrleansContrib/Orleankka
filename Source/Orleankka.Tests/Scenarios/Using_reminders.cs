using System;
using System.Linq;
using System.Threading.Tasks;

using NUnit.Framework;

namespace Orleankka.Scenarios
{
    using Testing;

    [Explicit, Category("Nightly")]
    [RequiresSilo(Fresh = true, DefaultKeepAliveTimeoutInMinutes = 1)]
    public class Using_reminders : ActorSystemScenario
    {
        [Test]
        public async void When_reminder_is_fired_an_instance_of_correct_actor_type_should_be_activated()
        {
            var actor = system.FreshActorOf<TestActor>();
            var hashcode = await actor.Ask<long>(new GetInstanceHashcode());
            
            await actor.Tell(new SetReminder{Period = TimeSpan.FromMinutes(1.5)});
            await Task.Delay(TimeSpan.FromMinutes(2.0));

            Assert.True(await actor.Ask<bool>(new HasBeenReminded()));
            Assert.AreNotEqual(hashcode, await actor.Ask<long>(new GetInstanceHashcode()));
        } 
    }
}
