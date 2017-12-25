using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using NUnit.Framework;
using Orleans.Providers;

namespace Orleankka.Features
{
    namespace Stateful_actors
    {
        using Meta;
        using Testing;

        [Serializable]
        public class GetStorageProviderInvocations : Query<List<string>> {}

        [Serializable]
        public class ClearState : Command {}

        [StorageProvider(ProviderName = "Test")]
        public class TestActor : StatefulActor<TestState>
        {
            public override async Task OnActivate()
            {
                await base.OnActivate();

                await ReadState();
                await WriteState();
            }

            async Task On(ClearState _) => await ClearState();

            List<string> On(GetStorageProviderInvocations _) => State.Invocations;
        }

        [Serializable]
        public class TestState
        {
            public readonly List<string> Invocations = new List<string>();
        }

        public class TestActorStorageProvider : ActorStorageProvider<TestState>
        {
            public override Task ReadStateAsync(string type, string id, TestState state)
            {
                state.Invocations.Add($"{nameof(ReadStateAsync)}:{type}:{id}");
                return Task.CompletedTask;
            }

            public override Task WriteStateAsync(string type, string id, TestState state)
            {
                state.Invocations.Add($"{nameof(WriteStateAsync)}:{type}:{id}");
                return Task.CompletedTask;
            }

            public override Task ClearStateAsync(string type, string id, TestState state)
            {
                return Task.CompletedTask;
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
            public async Task When_using_custom_actor_storage_service()
            {
                var actor = system.FreshActorOf<TestActor>();

                var expected = new List<string>
                {
                    $"ReadStateAsync:{typeof(TestActor).FullName}:{actor.Path.Id}", // by default on activate
                    $"ReadStateAsync:{typeof(TestActor).FullName}:{actor.Path.Id}",
                    $"WriteStateAsync:{typeof(TestActor).FullName}:{actor.Path.Id}",
                };

                var invocations = await actor.Ask(new GetStorageProviderInvocations());
                CollectionAssert.AreEqual(expected, invocations);

                await actor.Tell(new ClearState());
                Assert.That(await actor.Ask(new GetStorageProviderInvocations()), 
                   Has.Count.EqualTo(0));
            }
        }
    }
}