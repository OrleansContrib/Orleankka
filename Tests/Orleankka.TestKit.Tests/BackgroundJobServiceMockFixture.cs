using System;
using System.Linq;
using System.Threading.Tasks;

using NUnit.Framework;

using Orleankka.TestKit;

using Orleans;

namespace Orleankka.TestKit
{
    interface ITestBackgroundJobActor : IActorGrain, IGrainWithStringKey
    { }

    class TestBackgroundJobActor : DispatchActorGrain, TestKit.ITestBackgroundJobActor
    {
        public TestBackgroundJobActor(string id = null, IActorRuntime runtime = null) : base(id, runtime)
        { }
        
        public void Handle(string jobName) => Jobs.Run(jobName, t => Task.CompletedTask);
    }

    [TestFixture]
    public class BackgroundJobServiceMockFixture
    {
        [SetUp]
        public void SetUpTest()
        {
            runtime = new ActorRuntimeMock();
            actor = new TestBackgroundJobActor(Guid.NewGuid().ToString(), runtime);
        }

        ActorRuntimeMock runtime;
        TestBackgroundJobActor actor;

        [Test]
        public void Records_scheduled_job()
        {
            actor.Handle("test");
            Assert.IsTrue(runtime.Jobs.RecordedJobs.Count() == 1);
        }

        [Test]
        public async Task Recorded_job_can_be_started()
        {
            actor.Handle("test");
            var job = runtime.Jobs.RecordedJobs.First();
            
            await job.Invoke();
            Assert.IsTrue(job.IsTerminated);

            var host = runtime.Jobs.Host;
            Assert.IsTrue(host.RecordedMessages.Count() == 1);
        }
    }
}