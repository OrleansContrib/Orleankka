using System;
using System.Threading.Tasks;

using NUnit.Framework;
using Orleans;

namespace Orleankka.Features
{
    using CSharp;

    namespace Handler_wiring
    {
        using Core;
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
            public async void Auto_wires_any_method_named_On_or_Handle_which_has_single_argument()
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

                Assert.DoesNotThrow(
                    async () => await actor.Tell(new NonPublicHandlerMessage()));

                Assert.Throws<Dispatcher.HandlerNotFoundException>(
                    async () => await actor.Tell(new NonSingleArgumentHandlerMessage()));
            }

            [Test, Ignore("Write separate tests for dispatcher")]
            public void Calls_fallback_when_handler_not_found()
            {
                //var actor = new TestActor {Type = ActorType.From(typeof(TestActor))};
                var actor = new TestActor();

                var unknownMessage = new UnknownMessage();
                object bouncedMessage = null;

                Assert.DoesNotThrow(() => actor.Dispatch(unknownMessage, message => bouncedMessage = message));
                Assert.That(bouncedMessage, Is.SameAs(unknownMessage));
            }

            [Serializable]
            class UnknownMessage
            {}

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
                public void On(OnVoidMessage m){}
                public Task On(OnAsyncVoidMessage m) => TaskDone.Done;
                public string On(OnResultMessage m) => "42";
                public Task<string> On(OnAsyncResultMessage m) => Task.FromResult("42");

                public void Handle(HandleVoidMessage m){}
                public Task Handle(HandleAsyncVoidMessage m) => TaskDone.Done;
                public string Handle(HandleResultMessage m) => "42";
                public Task<string> Handle(HandleAsyncResultMessage m) => Task.FromResult("42");

                void On(NonPublicHandlerMessage m){}
                public void Handle(NonSingleArgumentHandlerMessage m, int i){}

                public void Dispatch(object message, Action<object> fallback)
                {
                    base.Dispatch(message, m =>
                    {
                        fallback(m);
                        return null;
                    });
                }
            }
        }
    }
}