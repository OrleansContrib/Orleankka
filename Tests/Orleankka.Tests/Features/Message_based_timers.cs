using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

using Orleans.CodeGeneration;
using Orleans.Concurrency;

namespace Orleankka.Features
{
    namespace Message_based_timers
    {
        using Meta;
        using Testing;

        [Serializable]
        public class SetOneOffTimer : Command {}

        [Serializable]
        public class SetPeriodicTimer : Command
        {
            public readonly TimeSpan Period;
            public SetPeriodicTimer(TimeSpan period) => Period = period;
        }

        [Serializable] 
        public class NumberOfTimesTimerTicked : Query<int> {}

        public interface ITestInterleavedTimerMessageActor : IActorGrain
        {}

        public class TestInterleavedTimerMessageActor : ActorGrain, ITestInterleavedTimerMessageActor
        {
            int ticked;

            public override Task<object> Receive(object message)
            {
                switch (message)
                {
                    case NumberOfTimesTimerTicked x: 
                        if (Timers.IsRegistered("test"))
                            Timers.Unregister("test");
                        return TaskResult.From(ticked);
                    
                    case SetOneOffTimer x: 
                        Timers.Register("test", TimeSpan.FromMilliseconds(10));
                        return TaskResult.Done;

                    case SetPeriodicTimer x: 
                        Timers.Register("test", x.Period, x.Period);
                        return TaskResult.Done;
                    
                    case Timer x:
                        ticked++;
                        return TaskResult.Done;                        
                }

                return TaskResult.Unhandled;
            }
        }

        [Serializable] public class SetCustomTimerMessage : Command {}
        [Serializable] public class CustomTimerMessageReceived : Query<bool> {}
        [Serializable] public class CustomTimerMessage {}

        public interface ITestCustomTimerMessageActor : IActorGrain
        {}

        public class TestCustomTimerMessageActor : ActorGrain, ITestCustomTimerMessageActor
        {
            bool customMessageReceived;

            public override Task<object> Receive(object message)
            {
                switch (message)
                {
                    case CustomTimerMessageReceived x: 
                        return TaskResult.From(customMessageReceived);
                    
                    case SetCustomTimerMessage x: 
                        Timers.Register("test", TimeSpan.FromMilliseconds(10), message: new CustomTimerMessage());
                        return TaskResult.Done;
                    
                    case CustomTimerMessage x:
                        customMessageReceived = true;
                        return TaskResult.Done;                        
                }

                return TaskResult.Unhandled;
            }
        }

        [Serializable] public class SetTimer : Command
        {
            public readonly TimeSpan Period;
            public readonly bool Interleave;
            public readonly bool FireAndForget;

            public SetTimer(TimeSpan period, bool interleave, bool fireAndForget)
            {
                Period = period;
                Interleave = interleave;
                FireAndForget = fireAndForget;
            }
        }

        public interface ITestFireAndForgetWithNonInterleavedTimerMessageActor : IActorGrain
        {}

        [MayInterleave("MayInterleave")]
        public class TestFireAndForgetWithNonInterleavedTimerMessageActor : ActorGrain, ITestFireAndForgetWithNonInterleavedTimerMessageActor
        {
            public static bool MayInterleave(InvokeMethodRequest req) => req.Arguments[0] is NumberOfTimesTimerTicked;

            readonly List<TaskCompletionSource<bool>> outstanding = new List<TaskCompletionSource<bool>>();

            public override async Task<object> Receive(object message)
            {
                switch (message)
                {
                    case NumberOfTimesTimerTicked _: 
                        Timers.Unregister("test");
                        outstanding.ForEach(x => x.SetResult(true));
                        return outstanding.Count;
                    
                    case SetTimer x:
                        Timers.Register("test", x.Period, x.Period, interleave: x.Interleave, fireAndForget: x.FireAndForget);
                        return Done;
                    
                    case Timer x:
                        var tcs = new TaskCompletionSource<bool>();
                        outstanding.Add(tcs);
                        await tcs.Task;
                        return Done;
                }

                return Unhandled;
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
            public async Task When_setting_interleaved_one_off_timer()
            {
                var actor = system.FreshActorOf<ITestInterleavedTimerMessageActor>();

                await actor.Tell(new SetOneOffTimer());
                Thread.Sleep(100);

                Assert.AreEqual(1, await actor.Ask(new NumberOfTimesTimerTicked()));
            }
            
            [Test]
            public async Task When_setting_interleaved_periodic_timer()
            {
                var actor = system.FreshActorOf<ITestInterleavedTimerMessageActor>();

                const int times = 10;
                var period = TimeSpan.FromMilliseconds(15);

                await actor.Tell(new SetPeriodicTimer(period));
                Thread.Sleep(period * times * 2);

                Assert.That(await actor.Ask(new NumberOfTimesTimerTicked()), Is.GreaterThanOrEqualTo(times));
            }

            [Test]
            public async Task When_sending_custom_timer_message()
            {
                var actor = system.FreshActorOf<ITestCustomTimerMessageActor>();

                await actor.Tell(new SetCustomTimerMessage());
                Thread.Sleep(100);

                Assert.True(await actor.Ask(new CustomTimerMessageReceived()));
            }
            
            [Test]
            public async Task When_sending_interleaved_fire_and_forget_message()
            {
                var actor = system.FreshActorOf<ITestFireAndForgetWithNonInterleavedTimerMessageActor>();

                var period = TimeSpan.FromMilliseconds(15);
                await actor.Tell(new SetTimer(period, interleave: true, fireAndForget: true));
                
                const int times = 10;
                Thread.Sleep(period * times * 2);

                Assert.That(await actor.Ask(new NumberOfTimesTimerTicked()), Is.GreaterThanOrEqualTo(times));
            }
            
            [Test]
            public async Task When_sending_non_interleaved_fire_and_forget_message()
            {
                var actor = system.FreshActorOf<ITestFireAndForgetWithNonInterleavedTimerMessageActor>();

                var period = TimeSpan.FromMilliseconds(15);
                await actor.Tell(new SetTimer(period, interleave: false, fireAndForget: true));
                
                const int times = 10;
                Thread.Sleep(period * times * 2);

                Assert.That(await actor.Ask(new NumberOfTimesTimerTicked()), Is.EqualTo(1));
            }
        }
    }
}