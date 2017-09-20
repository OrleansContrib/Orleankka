using System;
using System.Threading;

using NUnit.Framework;
using Orleans;

namespace Orleankka.Features
{
    namespace Message_sending_timers
    {
        using Meta;
        using Testing;

        [Serializable]
        public class SetMessageSendingTimer : Command
        { }
        [Serializable]
        public class MessageSentByTimer : Command
        { }

        [Serializable]
        public class NumberOfMessagesTimerSent : Query<int>
        {}

        public class TestActor : Actor
        {
            int fired;

            void On(SetMessageSendingTimer cmd)
            {
                Timers.Register("test", TimeSpan.FromMilliseconds(10), new MessageSentByTimer());
            }
            void On(MessageSentByTimer cmd)
            {
                fired++;
            }

            int On(NumberOfMessagesTimerSent q) => fired;
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
            public async void When_setting_message_sending_timer()
            {
                var actor = system.FreshActorOf<TestActor>();

                await actor.Tell(new SetMessageSendingTimer());
                Thread.Sleep(100);

                Assert.AreEqual(1, await actor.Ask(new NumberOfMessagesTimerSent()));
            }
        }
    }
}