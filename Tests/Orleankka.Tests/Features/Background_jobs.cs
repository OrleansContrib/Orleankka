using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

using Orleans;

namespace Orleankka.Features
{
    namespace Background_jobs
    {
        using Meta;
        using Testing;
        using Utility;
        using Services;
        using Orleans.Metadata;

        [Serializable] public abstract class JobMessage : Message
        {
            public string Name;
        }

        [Serializable] public class RunOneOffJob : JobMessage {}
        [Serializable] public class RunFailingOneOffJob : JobMessage
        {
            public int Failures;
            public int Retries;
        }

        [Serializable]
        public class RunLoopedJob : JobMessage
        {
            public TimeSpan LoopDelay;
        }

        [Serializable] public class TerminateJob : JobMessage {}
        [Serializable] public class NumberOfTimesJobRan : JobMessage, Query<int> {}
        [Serializable] public class NumberOfTimesJobTerminated : JobMessage,  Query<int> {}

        [DefaultGrainType("bg-test")]
        public interface ITestActor : IActorGrain, IGrainWithStringKey
        {}

        [GrainType("bg-test")]
        public class TestActor : DispatchActorGrain, ITestActor
        {
            readonly Dictionary<string, int> runs = new Dictionary<string, int>();
            readonly Dictionary<(string name, int id), int> terminations = new Dictionary<(string, int), int>();

            int failures;
            int retries;
            BackgroundJob job;

            void RecordJobRun(string id)
            {
                var fires = runs.Find(id, default);
                runs[id] = fires + 1;
            }

            void On(TerminateJob cmd) => Jobs.Terminate(cmd.Name);

            void On(RunOneOffJob cmd)
            {
                Jobs.Run(cmd.Name, _ =>
                {
                    RecordJobRun(cmd.Name);
                    return Task.CompletedTask;
                });
            }

            void On(RunFailingOneOffJob cmd)
            {
                failures = cmd.Failures;
                retries = cmd.Retries;

                job = Jobs.Run(cmd.Name, _ =>
                {
                    RecordJobRun(cmd.Name);
                    
                    if (failures-- > 0)
                        throw new ApplicationException("boom!");

                    return Task.CompletedTask;
                });
            }

            void On(RunLoopedJob cmd)
            {
                job = Jobs.Run(cmd.Name, async token =>
                {
                    while (!token.IsTerminationRequested)
                    {
                        RecordJobRun(cmd.Name);
                        await Task.Delay(cmd.LoopDelay);
                    }
                });
            }

            void On(BackgroundJobFailed e)
            {
                if (job.Id != e.Id) // correlating is easy
                    return;

                if (e.Failures <= retries)
                {
                    job.Retry();
                    return;
                }
                
                job.Terminate();
            }

            void On(BackgroundJobTerminated e)
            {
                var key = (e.Name, e.Id);
                var counter = terminations.Find(key, default);
                terminations[key] = counter + 1;
            }

            int On(NumberOfTimesJobRan q) => runs.Find(q.Name, 0);
            int On(NumberOfTimesJobTerminated q) => terminations.FirstOrDefault(x => x.Key.name == q.Name).Value;
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
            public async Task When_running_one_off_job()
            {
                var actor = system.FreshActorOf<ITestActor>();

                await actor.Tell(new RunOneOffJob {Name="test"});

                await Try.Until(async ()=> await actor.Ask(new NumberOfTimesJobRan{Name="test"}) == 1);
                await Try.Until(async ()=> await actor.Ask(new NumberOfTimesJobTerminated{Name="test"}) == 1);
            }
            
            [Test]
            public async Task When_running_failing_one_off_job()
            {
                var actor = system.FreshActorOf<ITestActor>();

                await actor.Tell(new RunFailingOneOffJob
                {
                    Name = "test", 
                    Failures = 2,
                    Retries = 3
                });

                await Try.Until(async ()=> await actor.Ask(new NumberOfTimesJobRan{Name="test"}) == 3);
                await Try.Until(async ()=> await actor.Ask(new NumberOfTimesJobTerminated{Name="test"}) == 1);
            }

            [Test]
            public async Task When_terminating_loop_based_job()
            {
                var actor = system.FreshActorOf<ITestActor>();

                var loopDelay = TimeSpan.FromMilliseconds(50);
                await actor.Tell(new RunLoopedJob
                {
                    Name = "test", 
                    LoopDelay = loopDelay
                });

                const int loops = 5;
                var due = loopDelay * loops;

                await Try.Until(async () => await
                    actor.Ask(new NumberOfTimesJobRan {Name = "test"}) >= loops, due);

                await actor.Tell(new TerminateJob{Name="test"});
                
                await Try.Until(async ()=> 
                    await actor.Ask(new NumberOfTimesJobTerminated{Name="test"}) == 1);
            }
        }
    }
}