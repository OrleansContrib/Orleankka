using System;
using System.Threading.Tasks;

using NUnit.Framework;

namespace Orleankka.Features
{
    namespace Pattern_matching
    {
        using Testing;

        [Serializable] public class ReturnWrappedDone {}
        [Serializable] public class ReturnWrappedTask {}

        public interface ITestReturnsWrappedDoneActor : IActorGrain {}

        public class TestReturnsWrappedDoneActor : ActorGrain, ITestReturnsWrappedDoneActor
        {
            public override async Task<object> Receive(object message)
            {
                switch (message)
                {
                    case ReturnWrappedDone _:
                        return Done;
                    case ReturnWrappedTask _:
                        return Task.FromResult(1);
                }

                return await base.Receive(message);
            }
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
            }

            [Test]
            public async Task When_wrapped_done_is_returned()
            {
                var actor = system.FreshActorOf<TestReturnsWrappedDoneActor>();
                Assert.IsNull(await actor.Ask<object>(new ReturnWrappedDone()));
            }
            
            [Test]
            public void When_wrapped_task_is_returned()
            {
                var actor = system.FreshActorOf<TestReturnsWrappedDoneActor>();
                Assert.ThrowsAsync<InvalidOperationException>(async ()=> await actor.Ask<object>(new ReturnWrappedTask()));
            }
        }
    }
}