﻿using System;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

using Orleans;

namespace Orleankka.Features
{
    namespace One_off_timers
    {
        using Meta;
        using Orleans.Metadata;

        using Testing;

        [Serializable]
        public class SetOneOffTimer : Command
        {}

        [Serializable]
        public class NumberOfTimesTimerFired : Query<int>
        {}

        [DefaultGrainType("oneoff-timer-test")]
        public interface ITestActor : IActorGrain, IGrainWithStringKey
        {}

        [GrainType("oneoff-timer-test")]
        public class TestActor : DispatchActorGrain, ITestActor
        {
            int fired;

            void On(SetOneOffTimer cmd)
            {
                Timers.Register("test", TimeSpan.FromMilliseconds(10), () =>
                {
                    fired++;
                    return Task.CompletedTask;
                });
            }

            int On(NumberOfTimesTimerFired q) => fired;
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
            public async Task When_setting_one_off_timer()
            {
                var actor = system.FreshActorOf<ITestActor>();

                await actor.Tell(new SetOneOffTimer());
                Thread.Sleep(100);

                Assert.AreEqual(1, await actor.Ask(new NumberOfTimesTimerFired()));
            }
        }
    }
}