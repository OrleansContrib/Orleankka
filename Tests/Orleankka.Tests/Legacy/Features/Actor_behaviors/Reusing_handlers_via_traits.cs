using System.Collections.Generic;
using System.Threading.Tasks;

using NUnit.Framework;

using Orleans;

namespace Orleankka.Legacy.Features.Actor_behaviors
{
    namespace Reusing_handlers_via_traits
    {
        using Behaviors;
        using Orleans.Metadata;

        class X { }
        class Y { }

        [DefaultGrainType("reuse-handler-test")]
        interface ITestActor : IActorGrain, IGrainWithStringKey
        {}

        [GrainType("reuse-handler-test")]
        class TestActor : Actor, ITestActor
        {
            public readonly List<string> Events = new List<string>();

            public TestActor() : base(new MockRuntime()) {}

            [Trait] void ATrait() => this.OnReceive<X>(x => Events.Add("OnReceiveX_ATrait"));
            [Trait] void BTrait() => this.OnReceive<Y>(x => Events.Add("OnReceiveY_BTrait"));

            [Behavior] public void CombineTraits()
            {
                this.Trait(ATrait, BTrait);
            }
        }

        [TestFixture]
        class Tests
        {
            TestActor actor;

            [SetUp]
            public void SetUp()
            {
                actor = new TestActor();
            }

            [Test]
            public async Task When_combining_traits()
            {
                actor.Behavior.Initial(actor.CombineTraits);
                actor.Events.Clear();

                await actor.OnReceive(new X());
                await actor.OnReceive(new Y());

                var expected = new[]
{
                    "OnReceiveX_ATrait",
                    "OnReceiveY_BTrait"
                };

                AssertEqual(expected, actor.Events);
            }

            static void AssertEqual(IEnumerable<string> expected, IEnumerable<string> actual) =>
                CollectionAssert.AreEqual(expected, actual);
        }
    }
}