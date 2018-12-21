using System;
using System.Threading.Tasks;

using NUnit.Framework;

using Orleans.Core;
using Orleans;
using Orleans.Runtime;
using Orleans.Storage;

namespace Orleankka.Features
{
    namespace Storage_provider_facet
    {
        using Facets;
        using Meta;
        using Testing;

        [Serializable] public class GetState : Query<string> {}

        public interface ITestActor : IActorGrain {}

        public class TestActor : ActorGrain, ITestActor
        {
            readonly IStorage<TestState> storage;

            public TestActor([UseStorageProvider("test")] IStorage<TestState> storage) => 
                this.storage = storage;

            public override Task<object> Receive(object message)
            {
                switch (message)
                {
                   case GetState _:
                       return TaskResult.From(storage.State.Data);
                }

                return TaskResult.Unhandled;
            }
        }

        public class TestState
        {
            public string Data;
        }

        public class TestStorageProvider : IGrainStorage
        {
            readonly string name;
            
            public TestStorageProvider(string name) => this.name = name;

            public Task ReadStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
            {
                var state = new TestState {Data = $"fromStorage-{name}-{grainReference.GetPrimaryKeyString()}"};
                grainState.State = state;
                return Task.CompletedTask;
            }

            public Task WriteStateAsync(string grainType, GrainReference grainReference, IGrainState grainState) => throw new NotImplementedException();
            public Task ClearStateAsync(string grainType, GrainReference grainReference, IGrainState grainState) => throw new NotImplementedException();
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
            public async Task When_activated()
            {
                var actor = system.FreshActorOf<ITestActor>();
                
                var state = await actor.Ask<string>(new GetState());
                
                Assert.AreEqual($"fromStorage-test-{actor.Path.Id}", state);
            }
        }
    }
}