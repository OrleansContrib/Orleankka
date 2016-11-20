using System;
using System.Threading.Tasks;

using NUnit.Framework;
using Orleans;

namespace Orleankka.Checks
{
    using Annotations;

    [TestFixture]
    public class DispatcherFixture
    {
        TestActor target;

        [SetUp]
        public void SetUp() => target = new TestActor();

        [Test]
        public async Task Auto_wires_any_method_which_has_single_argument_according_to_default_naming_conventions()
        {
            var dispatcher = new Dispatcher(typeof(TestActor));

            Assert.That(await dispatcher.Dispatch(target, new OnVoidMessage()), Is.Null);
            Assert.That(await dispatcher.Dispatch(target, new OnAsyncVoidMessage()), Is.Null);
            Assert.That(await dispatcher.Dispatch(target, new OnResultMessage()), Is.EqualTo("42"));
            Assert.That(await dispatcher.Dispatch(target, new OnAsyncResultMessage()), Is.EqualTo("42"));

            Assert.That(await dispatcher.Dispatch(target, new HandleVoidMessage()), Is.Null);
            Assert.That(await dispatcher.Dispatch(target, new HandleAsyncVoidMessage()), Is.Null);
            Assert.That(await dispatcher.Dispatch(target, new HandleResultMessage()), Is.EqualTo("42"));
            Assert.That(await dispatcher.Dispatch(target, new HandleAsyncResultMessage()), Is.EqualTo("42"));

            Assert.DoesNotThrow(
                async () => await dispatcher.Dispatch(target, new NonPublicHandlerMessage()));

            Assert.Throws<Dispatcher.HandlerNotFoundException>(
                async () => await dispatcher.Dispatch(target, new NonSingleArgumentHandlerMessage()));

            Assert.Throws<Dispatcher.HandlerNotFoundException>(
                async () => await dispatcher.Dispatch(target, "custom handler name"));
        }

        [Test]
        public void Calls_fallback_when_handler_not_found()
        {
            var dispatcher = new Dispatcher(typeof(TestActor));

            var unknownMessage = new UnknownMessage();
            object bouncedMessage = null;

            Assert.DoesNotThrow(async () => await dispatcher.Dispatch(target, unknownMessage, message =>
            {
                bouncedMessage = message;
                return Task.FromResult((object)42);
            }));

            Assert.That(bouncedMessage, Is.SameAs(unknownMessage));
        }

        [Test]
        public async Task Custom_naming_conventions()
        {
            var dispatcher = new Dispatcher(typeof(TestActor), new[] {"CustomHandler"});
            Assert.That(await dispatcher.Dispatch(target, "custom handler name"), Is.EqualTo(42));
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
        class TestActor
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

            int CustomHandler(string msg) => 42;
        }
    }
}