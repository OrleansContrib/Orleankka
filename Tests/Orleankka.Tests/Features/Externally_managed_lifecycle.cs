using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using NUnit.Framework;

using Orleans;

namespace Orleankka.Features
{
    namespace Externally_managed_lifecycle
    {
        using Meta;

        using Orleans.Metadata;

        using Testing;

        [Serializable] public class Activated : Query<int> {}
        [Serializable] public class GetInstanceHashcode : Query<int> {}
        
        [DefaultGrainType("ext-test")]
        public interface ITestActor : IActorGrain, IGrainWithStringKey {}
        
        [GrainType("ext-test")]
        public class TestActor : ActorGrain, ITestActor
        {
            int activated;

            public override Task<object> Receive(object message)
            {
                switch (message)
                {
                    case Activate _: 
                        activated++;
                        break;

                    case GetInstanceHashcode _: 
                        return Task.FromResult<object>(RuntimeHelpers.GetHashCode(this));

                    case Activated _: 
                        return Task.FromResult<object>(activated);

                    default: 
                        return TaskResult.Unhandled;
                }

                return TaskResult.Done;
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
            public async Task Activation_is_idempotent()
            {
                var actor = system.FreshActorOf<ITestActor>();
                
                await actor.Activate();
                await actor.Activate();

                Assert.AreEqual(1, await actor.Ask(new Activated()));
            }

            [Test]
            public async Task Deactivates_on_idle()
            {
                var actor = system.FreshActorOf<ITestActor>();
                
                await actor.Activate();
                var hash = await actor.Ask(new GetInstanceHashcode());

                await actor.Deactivate();
                Assert.AreNotEqual(hash, await actor.Ask(new GetInstanceHashcode()));
            }
        }
    }
}