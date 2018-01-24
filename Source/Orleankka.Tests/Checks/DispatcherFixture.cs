using System;
using System.Threading.Tasks;

using NUnit.Framework;

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

            Assert.That(await dispatcher.DispatchAsync(target, new OnVoidMessage()), Is.Null);
            Assert.That(await dispatcher.DispatchAsync(target, new OnAsyncVoidMessage()), Is.Null);
            Assert.That(await dispatcher.DispatchAsync(target, new OnResultMessage()), Is.EqualTo("42"));
            Assert.That(await dispatcher.DispatchAsync(target, new OnAsyncResultMessage()), Is.EqualTo("42"));

            Assert.That(await dispatcher.DispatchAsync(target, new HandleVoidMessage()), Is.Null);
            Assert.That(await dispatcher.DispatchAsync(target, new HandleAsyncVoidMessage()), Is.Null);
            Assert.That(await dispatcher.DispatchAsync(target, new HandleResultMessage()), Is.EqualTo("42"));
            Assert.That(await dispatcher.DispatchAsync(target, new HandleAsyncResultMessage()), Is.EqualTo("42"));

            Assert.DoesNotThrowAsync(
                async () => await dispatcher.DispatchAsync(target, new PrivateHandlerMessage()));

            Assert.DoesNotThrowAsync(
                async () => await dispatcher.DispatchAsync(target, new InternalHandlerMessage()));

            Assert.ThrowsAsync<Dispatcher.HandlerNotFoundException>(
                async () => await dispatcher.DispatchAsync(target, new NonSingleArgumentHandlerMessage()));

            Assert.ThrowsAsync<Dispatcher.HandlerNotFoundException>(
                async () => await dispatcher.DispatchAsync(target, "custom handler name"));
        }

        [Test]
        public void Calls_fallback_when_handler_not_found()
        {
            var dispatcher = new Dispatcher(typeof(TestActor));

            var unknownMessage = new UnknownMessage();
            object bouncedMessage = null;

            Assert.DoesNotThrowAsync(async () => await dispatcher.DispatchAsync(target, unknownMessage, message =>
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
            Assert.That(await dispatcher.DispatchAsync(target, "custom handler name"), Is.EqualTo(42));
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
        class PrivateHandlerMessage
        {}

        [Serializable]
        class InternalHandlerMessage
        {}

        [Serializable]
        class NonSingleArgumentHandlerMessage
        {}

        [UsedImplicitly]
        class TestActor
        {
            public void On(OnVoidMessage m){}
            public Task On(OnAsyncVoidMessage m) => Task.CompletedTask;
            public string On(OnResultMessage m) => "42";
            public Task<string> On(OnAsyncResultMessage m) => Task.FromResult("42");

            public void Handle(HandleVoidMessage m){}
            public Task Handle(HandleAsyncVoidMessage m) => Task.CompletedTask;
            public string Handle(HandleResultMessage m) => "42";
            public Task<string> Handle(HandleAsyncResultMessage m) => Task.FromResult("42");

            void On(PrivateHandlerMessage m){}
            internal void On(InternalHandlerMessage m){}

            public void Handle(NonSingleArgumentHandlerMessage m, int i){}
            int CustomHandler(string msg) => 42;
        }
    }
}