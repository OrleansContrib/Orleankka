using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using NUnit.Framework;

using Orleans;
using Orleans.Runtime;

namespace Orleankka.Features
{
    namespace Intercepting_requests
    {
        using Meta;
        using Orleans.Metadata;

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

        [Serializable]
        public class ItemData : Event
        {
            public string Text;
        }

        [DefaultGrainType("interceptor-test")]
        public interface ITestActor : IActorGrain, IGrainWithStringKey
        {}

        public abstract class TestActorBase : DispatchActorGrain
        {}

        /// middleware is set for the base actor type
        [GrainType("interceptor-test")]
        public class TestActor : TestActorBase, ITestActor
        {
            string text = "";

            void On(SetText cmd) => text = cmd.Text;
            string On(GetText q) => text;

            string On(CheckRef cmd) => (string) RequestContext.Get("SetByActorRefMiddleware");

            readonly List<string> fromStream = new List<string>();
            List<string> On(GetReceivedFromStream x) => fromStream;
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

        [DefaultGrainType("inside-interceptor-test")]
        public interface ITestInsideActor : IActorGrain, IGrainWithStringKey
        {}

        [GrainType("inside-interceptor-test")]
        public class TestInsideActor : DispatchActorGrain, ITestInsideActor
        {
            public async Task Handle(DoTell cmd) => await cmd.Target.Tell(cmd.Message);
            public Task<string> Handle(DoAsk query) => query.Target.Ask<string>(query.Message);
        }

        [Serializable]
        public class GetReceivedFromStream : Query<List<string>>
        {}

        public class TestActorMiddleware : ActorMiddleware
        {
            public override Task<object> Receive(ActorGrain actor, object message, Receive receiver)
            {
                switch (message)
                {
                    case SetText msg:
                        
                        if (msg.Text == "interrupt")
                            throw new InvalidOperationException();
                        
                        msg.Text += ".intercepted";
                        break;
                }

                return base.Receive(actor, message, receiver);
            }
        }

        public class TestActorRefMiddleware : ActorRefMiddleware
        {
            public override Task<object> Receive(ActorPath actor, object message, Receive receiver)
            {
                if (message is CheckRef)
                    RequestContext.Set("SetByActorRefMiddleware", "it works!");

                return base.Receive(actor, message, receiver);
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
            public async Task Client_to_actor()
            {
                var actor = system.FreshActorOf<ITestActor>();

                await actor.Tell(new SetText {Text = "c-a"});
                Assert.AreEqual("c-a.intercepted", await actor.Ask(new GetText()));
            }

            [Test]
            public async Task Actor_to_actor()
            {
                var one = system.FreshActorOf<ITestInsideActor>();
                var another = system.FreshActorOf<ITestActor>();

                await one.Tell(new DoTell {Target = another, Message = new SetText {Text = "a-a"}});
                Assert.AreEqual("a-a.intercepted", await one.Ask(new DoAsk {Target = another, Message = new GetText()}));
            }

            [Test]
            public async Task Interrupting_requests()
            {
                var actor = system.FreshActorOf<ITestActor>();

                Assert.ThrowsAsync<InvalidOperationException>(async ()=> await 
                    actor.Tell(new SetText { Text = "interrupt" }));

                Assert.AreEqual("", await actor.Ask(new GetText()));
            }

            [Test]
            public async Task Intercepting_actor_ref()
            {
                var actor = system.FreshActorOf<ITestActor>();
                var result = await actor.Ask<string>(new CheckRef());
                Assert.That(result, Is.EqualTo("it works!"));
            }
        }
    }
}