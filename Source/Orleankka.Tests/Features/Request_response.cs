using System;
using System.Linq;
using System.Threading.Tasks;

using NUnit.Framework;

namespace Orleankka.Features
{
    namespace Request_response
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
                Assert.AreEqual("c-a", await actor.Ask(new GetText()));
            }

            [Test]
            public async void Actor_to_actor()
            {
                var one = system.FreshActorOf<TestInsideActor>();
                var another = system.FreshActorOf<TestActor>();

                await one.Tell(new DoTell {Target = another, Message = new SetText {Text = "a-a"}});
                Assert.AreEqual("a-a", await one.Ask(new DoAsk {Target = another, Message = new GetText()}));
            }
        }
    }
}