using System;
using System.Linq;
using System.Threading.Tasks;

using NUnit.Framework;
using Orleans;

namespace Orleankka.Features
{
    namespace Handler_wiring
    {
        using Testing;
        using Annotations;

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
            public async void Auto_wires_any_public_method_named_On_or_Handle_which_has_single_argument()
            {
                var actor = system.FreshActorOf<TestActor>();

                Assert.That(await actor.Ask<object>(new OnVoidMessage()), Is.Null);
                Assert.That(await actor.Ask<object>(new OnAsyncVoidMessage()), Is.Null);
                Assert.That(await actor.Ask<string>(new OnResultMessage()), Is.EqualTo("42"));
                Assert.That(await actor.Ask<string>(new OnAsyncResultMessage()), Is.EqualTo("42"));

                Assert.That(await actor.Ask<object>(new HandleVoidMessage()), Is.Null);
                Assert.That(await actor.Ask<object>(new HandleAsyncVoidMessage()), Is.Null);
                Assert.That(await actor.Ask<string>(new HandleResultMessage()), Is.EqualTo("42"));
                Assert.That(await actor.Ask<string>(new HandleAsyncResultMessage()), Is.EqualTo("42"));

                Assert.Throws<Dispatcher.HandlerNotFoundException>(
                    async () => await actor.Tell(new NonPublicHandlerMessage()));

                Assert.Throws<Dispatcher.HandlerNotFoundException>(
                    async () => await actor.Tell(new NonSingleArgumentHandlerMessage()));
            }

            [Serializable]
            class OnVoidMessage
            {}

            [Serializable]
            class OnAsyncVoidMessage
            {}

            [Serializable]
            class OnResultMessage
            {}

            [Serializable]
            class OnAsyncResultMessage
            {}

            [Serializable]
            class HandleVoidMessage
            {}

            [Serializable]
            class HandleAsyncVoidMessage
            {}

            [Serializable]
            class HandleResultMessage
            {}

            [Serializable]
            class HandleAsyncResultMessage
            {}

            [Serializable]
            class NonPublicHandlerMessage
            {}

            [Serializable]
            class NonSingleArgumentHandlerMessage
            {}

            [UsedImplicitly]
            class TestActor : Actor
            {
                public void On(OnVoidMessage m)
                {
                    
                }

                public Task On(OnAsyncVoidMessage m)
                {
                    return TaskDone.Done;
                }

                public string On(OnResultMessage m)
                {
                    return "42";
                }

                public Task<string> On(OnAsyncResultMessage m)
                {
                    return Task.FromResult("42");
                }

                public void Handle(HandleVoidMessage m)
                {}

                public Task Handle(HandleAsyncVoidMessage m)
                {
                    return TaskDone.Done;
                }

                public string Handle(HandleResultMessage m)
                {
                    return "42";
                }

                public Task<string> Handle(HandleAsyncResultMessage m)
                {
                    return Task.FromResult("42");
                }

                void On(NonPublicHandlerMessage m)
                {}

                public void Handle(NonSingleArgumentHandlerMessage m, int i)
                {}
            }
        }
    }
}