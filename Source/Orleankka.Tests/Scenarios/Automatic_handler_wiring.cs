using System;
using System.Linq;
using System.Threading.Tasks;

using NUnit.Framework;
using Orleans;

namespace Orleankka.Scenarios
{
    using Testing;

    [TestFixture]
    [RequiresSilo]
    public class Automatic_handler_wiring
    {
        readonly IActorSystem system = TestActorSystem.Instance;

        [Test]
        public async void Should_automtically_wire_any_public_handler_with_single_arg_named_On_or_Handle()
        {
            var actor = system.FreshActorOf<TestAutoWiredHandlerActor>();
            
            Assert.That(await actor.Ask<object>(new OnVoidMessage()), Is.Null);
            Assert.That(await actor.Ask<object>(new OnAsyncVoidMessage()), Is.Null);
            Assert.That(await actor.Ask<string>(new OnResultMessage()), Is.EqualTo("42"));
            Assert.That(await actor.Ask<string>(new OnAsyncResultMessage()), Is.EqualTo("42"));
            
            Assert.That(await actor.Ask<object>(new HandleVoidMessage()), Is.Null);
            Assert.That(await actor.Ask<object>(new HandleAsyncVoidMessage()), Is.Null);
            Assert.That(await actor.Ask<string>(new HandleResultMessage()), Is.EqualTo("42"));
            Assert.That(await actor.Ask<string>(new HandleAsyncResultMessage()), Is.EqualTo("42"));

            Assert.Throws<Dispatcher.HandlerNotFoundException>(
                async ()=> await actor.Tell(new NonPublicHandlerMessage()));

            Assert.Throws<Dispatcher.HandlerNotFoundException>(
                async ()=> await actor.Tell(new NonSingleArgumentHandlerMessage()));
        }

        class OnVoidMessage {}
        class OnAsyncVoidMessage {}
        class OnResultMessage {}
        class OnAsyncResultMessage {}

        class HandleVoidMessage {}
        class HandleAsyncVoidMessage { }
        class HandleResultMessage { }
        class HandleAsyncResultMessage { }

        class NonPublicHandlerMessage {}
        class NonSingleArgumentHandlerMessage {}

        class TestAutoWiredHandlerActor : Actor
        {
            public void On(OnVoidMessage m) 
            {}
            
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