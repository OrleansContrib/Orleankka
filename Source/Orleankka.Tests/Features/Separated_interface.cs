using System;
using System.Threading.Tasks;

using NUnit.Framework;

namespace Orleankka.Features
{
    namespace Separated_interface
    {
        using Meta;
        using Testing;
        using Core;
        using Core.Endpoints;

        [Serializable]
        public class SetText : Command
        {
            public string Text;
        }

        [Serializable]
        public class GetText : Query<string>
        {}

        public interface ITestActorEndpoint : IActorEndpoint {}
        public class     TestActorEndpoint  : ActorEndpoint<TestActor>, ITestActorEndpoint {}

        public class TestActor : Actor
        {
            string text = "";

            string On(GetText q) => text;
            void On(SetText cmd) => text = cmd.Text;
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

        public interface ITestInsideActorEndpoint : IActorEndpoint {}
        public class     TestInsideActorEndpoint  : ActorEndpoint<TestInsideActor>, ITestInsideActorEndpoint {}

        public class TestInsideActor : Actor
        {
            async Task Handle(DoTell cmd) => await cmd.Target.Tell(cmd.Message);
            Task<string> Handle(DoAsk query) => query.Target.Ask<string>(query.Message);
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
                var actor = system.FreshActorOf<ITestActorEndpoint>();

                await actor.Tell(new SetText {Text = "c-a"});
                Assert.AreEqual("c-a", await actor.Ask(new GetText()));
            }

            [Test]
            public async void Actor_to_actor()
            {
                var one = system.FreshActorOf<ITestInsideActorEndpoint>();
                var another = system.FreshActorOf<ITestActorEndpoint>();

                await one.Tell(new DoTell {Target = another, Message = new SetText {Text = "a-a"}});
                Assert.AreEqual("a-a", await one.Ask(new DoAsk {Target = another, Message = new GetText()}));
            }
        }
    }
}