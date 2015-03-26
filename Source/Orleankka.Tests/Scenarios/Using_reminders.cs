using System;
using System.Linq;
using System.Threading.Tasks;

using NUnit.Framework;

namespace Orleankka.Scenarios
{
    using Testing;

    [RequiresSilo]
    public class Using_reminders : ActorSystemScenario
    {
        [Test, Explicit]
        public async void When_reminder_is_fired_an_instance_of_correct_actor_type_should_be_activated()
        {
            var actor = system.FreshActorOf<TestActor>();
            var hashcode = await actor.Ask<long>(new GetInstanceHashcode());
            
            await actor.Tell(new SetReminder());
            await Task.Delay(TimeSpan.FromSeconds(90));

            Assert.True(await actor.Ask<bool>(new HasBeenReminded()));
            Assert.AreNotEqual(hashcode, await actor.Ask<long>(new GetInstanceHashcode()));
        } 
    }
}
