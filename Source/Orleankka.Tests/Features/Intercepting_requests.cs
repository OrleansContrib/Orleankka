using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using NUnit.Framework;
using Orleans.Runtime;

namespace Orleankka.Features
{
    namespace Intercepting_requests
    {
        using Meta;
        using Testing;

        [Serializable]
        public class SetText : Command
        {
            public string Text;
        }

        [Serializable]
        public class GetText : Query<string>
        {}

        [Serializable]
        public class CheckRef : Query<string>
        {}

        [Invoker("test_actor_interception")]
        public class TestActor : Actor
        {
            string text = "";

            void On(SetText cmd) => text = cmd.Text;
            string On(GetText q) => text;

            string On(CheckRef cmd) => (string) RequestContext.Get("SetByActorRefInvoker");
        }

        [Serializable]
        public class DoTell : Command
        {
            public ActorRef Target;
            public object Message;
        }

        [Serializable]
        public class DoAsk : Query<string>
        {
            public ActorRef Target;
            public object Message;
        }

        public class TestInsideActor : Actor
        {
            public async Task Handle(DoTell cmd)
            {
                await cmd.Target.Tell(cmd.Message);
            }

            public Task<string> Handle(DoAsk query)
            {
                return query.Target.Ask<string>(query.Message);
            }
        }

        [Serializable]
        public class Subscribe : Command
        {
            public StreamRef Stream;
        }

        [Serializable]
        public class Received : Query<List<string>>
        {}

        [Invoker("test_stream_interception")]
        class TestStreamActor : Actor
        {
            readonly List<string> received = new List<string>();
            List<string> On(Received x) => received;

            Task On(Subscribe x) => x.Stream.Subscribe(this);
            void On(string x) => received.Add(x);
        }

        public class TestActorInterceptionInvoker : ActorInvoker
        {
            public override Task<object> OnReceive(Actor actor, object message)
            {
                var setText = message as SetText;
                if (setText == null)
                    return base.OnReceive(actor, message);

                if (setText.Text == "interrupt")
                    throw new InvalidOperationException();

                setText.Text += ".intercepted";
                return base.OnReceive(actor, message);
            }
        }

        public class TestStreamInterceptionInvoker : ActorInvoker
        {
            public override Task<object> OnReceive(Actor actor, object message)
            {
                var item = message as string;                    
                return base.OnReceive(actor, item == null ? message : item + ".intercepted");
            }
        }

        public class TestActorRefInvoker : ActorRefInvoker
        {
            public override Task<TResult> Send<TResult>(ActorPath actor, object message, Func<object, Task<object>> invoke)
            {
                if (message is CheckRef)
                    RequestContext.Set("SetByActorRefInvoker", "it works!");

                return base.Send<TResult>(actor, message, invoke);
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
            public async void Client_to_actor()
            {
                var actor = system.FreshActorOf<TestActor>();

                await actor.Tell(new SetText {Text = "c-a"});
                Assert.AreEqual("c-a.intercepted", await actor.Ask(new GetText()));
            }

            [Test]
            public async void Actor_to_actor()
            {
                var one = system.FreshActorOf<TestInsideActor>();
                var another = system.FreshActorOf<TestActor>();

                await one.Tell(new DoTell {Target = another, Message = new SetText {Text = "a-a"}});
                Assert.AreEqual("a-a.intercepted", await one.Ask(new DoAsk {Target = another, Message = new GetText()}));
            }

            [Test]
            public async void Interrupting_requests()
            {
                var actor = system.FreshActorOf<TestActor>();

                Assert.Throws<InvalidOperationException>(async ()=> await 
                    actor.Tell(new SetText { Text = "interrupt" }));

                Assert.AreEqual("", await actor.Ask(new GetText()));
            }

            [Test]
            public async void Intercepting_stream_messages()
            {
                var stream = system.StreamOf("sms", "test-stream-interception");
                
                var actor = system.FreshActorOf<TestStreamActor>();
                await actor.Tell(new Subscribe {Stream = stream});

                await stream.Push("foo");
                await Task.Delay(TimeSpan.FromMilliseconds(10));

                var received = await actor.Ask(new Received());
                Assert.That(received.Count, Is.EqualTo(1));
                Assert.That(received[0], Is.EqualTo("foo.intercepted"));
            }

            [Test]
            public async void Intercepting_actor_ref()
            {
                var actor = system.FreshActorOf<TestActor>();
                var result = await actor.Ask<string>(new CheckRef());
                Assert.That(result, Is.EqualTo("it works!"));
            }
        }
    }
}