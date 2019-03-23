using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orleankka.TestKit
{
    using Services;

    public class BackgroundJobServiceMock : IBackgroundJobService
    {
        readonly HashSet<BackgroundJob> jobs = new HashSet<BackgroundJob>();
        readonly TimerServiceMock timers;

        int jid;

        public BackgroundJobServiceMock()
        {
            Host = new ActorRefMock(ActorPath.Parse("mock:self"));
            timers = new TimerServiceMock();
        }

        public ActorRefMock Host { get; }

        BackgroundJob IBackgroundJobService.Run(string name, Func<BackgroundJobToken, Task> job)
        {
            var j = new RecordedBackgroundJob(jobs, Host, timers, name, jid++, job);
            j.Schedule(TimeSpan.Zero);
            return j;
        }

        RecordedBackgroundJob[] Active() => jobs.Cast<RecordedBackgroundJob>().ToArray();
        BackgroundJob[] IBackgroundJobService.Active() => jobs.ToArray();        
    }

    public class RecordedBackgroundJob : BackgroundJob
    {
        readonly TimerServiceMock timers;

        protected internal RecordedBackgroundJob(HashSet<BackgroundJob> tracker, ActorRef host, TimerServiceMock timers, string name, int id, Func<BackgroundJobToken, Task> job)
            : base(tracker, host, timers, name, id, job)
        {
            this.timers = timers;
        }

        public async Task Invoke()
        {
            if (!IsActive)
                throw new InvalidOperationException("Can't invoke inactive job");

            var request = timers.Requests.First(x => x.Id == TimerUid);
            await request.Timer.CallbackTimer().Callback();
        }
    }
}