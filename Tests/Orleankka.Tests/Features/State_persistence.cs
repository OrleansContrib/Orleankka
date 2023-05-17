using System;
using System.Threading.Tasks;

using NUnit.Framework;

using Orleans.Core;
using Orleans;
using Orleans.Runtime;
using Orleans.Storage;

namespace Orleankka.Features
{
    namespace State_persistence
    {
        using Meta;
        using Orleans.Metadata;

        using Testing;

        [Serializable] public class GetState : Query<string> {}

        [DefaultGrainType("state-test")]
        public interface ITestActor : IActorGrain, IGrainWithStringKey {}

        [GrainType("state-test")]
        public class TestActor : ActorGrain, ITestActor
        {
            readonly IStorage<TestState> storage;

            public TestActor([PersistentState("#foo", "test")] IStorage<TestState> storage) => 
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

            public Task ReadStateAsync<T>(string type, GrainId id, IGrainState<T> grainState)
            {
                var stateName = type.Substring(type.IndexOf('#') + 1);
                var state = new TestState {Data = $"fromStorage-{name}-{stateName}-{id}"};
                grainState.State = (T)((object)state);
                return Task.CompletedTask;
            }

            public Task WriteStateAsync<T>(string tyope, GrainId id, IGrainState<T> grainState) => throw new NotImplementedException();
            public Task ClearStateAsync<T>(string type, GrainId id, IGrainState<T> grainState) => throw new NotImplementedException();
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
                
                Assert.AreEqual($"fromStorage-test-foo-{actor.Path.Id}", state);
            }
        }
    }
}