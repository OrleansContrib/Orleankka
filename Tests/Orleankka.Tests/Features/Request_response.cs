using System;
using System.Threading.Tasks;

using NUnit.Framework;
using Orleans;

namespace Orleankka.Features
{
    namespace Request_response
    {
        using Meta;
        using Testing;

        public record SetText(string Text) : Command;
        public record GetText : Query<string>;

        public interface ITestActor : IActorGrain, IGrainWithStringKey {}
        public class TestActor : DispatchActorGrain, ITestActor
        {
            string text = "";

            public void On(SetText cmd) => text = cmd.Text;
            public string On(GetText q) => text;
        }

        public record DoTell(ActorRef Target, object Message) : Command;
        public record DoAsk(ActorRef Target, object Message) : Query<string>;

        public interface ITestInsideActor : IActorGrain, IGrainWithStringKey {}
        public class TestInsideActor : DispatchActorGrain, ITestInsideActor
        {
            public async Task Handle(DoTell cmd) => await cmd.Target.Tell(cmd.Message);
            public Task<string> Handle(DoAsk query) => query.Target.Ask<string>(query.Message);
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

                await actor.Tell(new SetText("c-a"));
                Assert.AreEqual("c-a", await actor.Ask(new GetText()));
            }

            [Test]
            public async Task Actor_to_actor()
            {
                var one = system.FreshActorOf<ITestInsideActor>();
                var another = system.FreshActorOf<ITestActor>();

                await one.Tell(new DoTell(another, new SetText("a-a")));
                Assert.AreEqual("a-a", await one.Ask(new DoAsk(another, new GetText())));
            }
        }
    }
}