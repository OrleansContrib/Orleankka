using System;
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

        public interface ITestActor : IActor
        { }

        public class TestActor : Actor, ITestActor
        {
            string text = "";

            public void On(SetText cmd) => text = cmd.Text;
            public string On(GetText q) => text;
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

        public interface ITestInsideActor : IActor
        { }

        public class TestInsideActor : Actor, ITestInsideActor
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
            public async Task Client_to_actor()
            {
                var actor = system.FreshActorOf<ITestActor>();

                await actor.Tell(new SetText {Text = "c-a"});
                Assert.AreEqual("c-a", await actor.Ask(new GetText()));
            }

            [Test]
            public async Task Actor_to_actor()
            {
                var one = system.FreshActorOf<ITestInsideActor>();
                var another = system.FreshActorOf<ITestActor>();

                await one.Tell(new DoTell {Target = another, Message = new SetText {Text = "a-a"}});
                Assert.AreEqual("a-a", await one.Ask(new DoAsk {Target = another, Message = new GetText()}));
            }
        }
    }
}