using System;
using System.Threading.Tasks;

using NUnit.Framework;

namespace Orleankka.Features
{
    namespace Receive_result_behavior
    {
        using Testing;

        [Serializable] public class ReturnWrappedTask {}

        public interface ITestActor : IActorGrain {}

        public class TestActor : ActorGrain, ITestActor
        {
            public override async Task<object> Receive(object message)
            {
                switch (message)
                {
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
            [Test]
            public void When_wrapped_task_is_returned()
            {
                var actor = TestActorSystem.Instance.FreshActorOf<TestActor>();
                Assert.ThrowsAsync<InvalidOperationException>(async ()=> 
                    await actor.Ask<object>(new ReturnWrappedTask()));
            }
        }
    }
}