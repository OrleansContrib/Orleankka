using System;
using System.Threading.Tasks;

using NUnit.Framework;

namespace Orleankka.Features
{
    namespace Intercepting_requests
    {
        using Meta;
        using Cluster;
        using Testing;

        [Serializable]
        public class SetText : Command
        {
            public string Text;
        }

        [Serializable]
        public class GetText : Query<string>
        {}

        [Invoker("test")]
        public class TestActor : Actor
        {
            string text = "";

            public void On(SetText cmd)
            {
                text = cmd.Text;
            }

            public string On(GetText q)
            {
                return text;
            }
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

        public class TestInterceptor : IInterceptor
        {
            public void Install(IInvocationPipeline pipeline, object properties)
            {
                pipeline.Register("test", new TestInvoker());
            }

            class TestInvoker : ActorInvoker
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
        }
    }
}