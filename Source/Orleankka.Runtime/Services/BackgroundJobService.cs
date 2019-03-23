using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Orleans.Concurrency;

namespace Orleankka.Services
{
    using Utility;

    /// <summary>
    /// Like task manager (htop) but for background jobs. Allow scheduling of new jobs and termination
    /// of existing jobs. Tracks ony active jobs (the jobs which are either scheduled or running).
    /// <remarks>
    /// Failed or terminated jobs are considered inactive (detached) and could be accessed only via
    /// <see cref="BackgroundJob"/> handle returned by <see cref="Run"/> method
    /// </remarks>
    /// </summary>
    public interface IBackgroundJobService
    {
        /// <summary>
        /// Schedules new job to be run.
        /// </summary>
        /// <param name="name">The name of the job</param>
        /// <param name="job">The callback function to invoke</param>
        /// <returns></returns>
        BackgroundJob Run(string name, Func<BackgroundJobToken, Task> job);

        /// <summary>
        /// Returns all active jobs (scheduled or running)
        /// </summary>
        /// <returns>The sequence of <see cref="BackgroundJob"/> </returns>
        BackgroundJob[] Active();
    }

    /// <summary>
    /// Default runtime implementation of <see cref="IBackgroundJobService"/>
    /// </summary>
    class BackgroundJobService : IBackgroundJobService
    {
        readonly HashSet<BackgroundJob> jobs = new HashSet<BackgroundJob>();
        readonly ActorGrain host;

        int jid;

        internal BackgroundJobService(ActorGrain host)
        {
            Requires.NotNull(host, nameof(host));
            this.host = host;
        }

        public BackgroundJob Run(string name, Func<BackgroundJobToken, Task> job)
        {
            Requires.NotNull(name, nameof(name));
            Requires.NotNull(job, nameof(job));

            var j = new BackgroundJob(jobs, host.Self, host.Timers, name, jid++, job);
            j.Schedule(TimeSpan.Zero);

            return j;
        }

        public BackgroundJob[] Active() => jobs.ToArray();
    }

    /// <summary>
    /// Represents the concept of "background job". The job runs in interleaved mode (ie timer-based).
    /// </summary>
    [DebuggerDisplay("a->{ToString()}")]
    public class BackgroundJob
    {
        enum JobStatus
        {
            Scheduled = 0,
            Running,
            Failed,
            Terminated
        }

        /// <summary>
        /// The name of the job
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// The unique id of the job (PID)
        /// </summary>
        public readonly int Id;

        /// <summary>
        /// The number of times this job has failed
        /// </summary>
        public int Failures { get; private set; }

        /// <summary>
        /// Answers whether this job is active (ie, scheduled or running)
        /// </summary>
        public bool IsActive => IsScheduled || IsRunning;

        /// <summary>
        /// Answers whether this job is scheduled (not running yet)
        /// </summary>
        public bool IsScheduled => status == JobStatus.Scheduled;

        /// <summary>
        /// Answers whether this job is running
        /// </summary>
        public bool IsRunning => status == JobStatus.Running;

        /// <summary>
        /// Answers whether this job has been failed
        /// </summary>
        public bool IsFailed => status == JobStatus.Failed;

        /// <summary>
        /// Answers whether this job has been terminated, either by itself
        /// (ran to completion) or forced externally
        /// </summary>
        public bool IsTerminated => status == JobStatus.Terminated;

        /// <summary>
        /// Answers whether the job termination was requested
        /// </summary>
        public bool IsTerminationRequested => jts.Token.IsTerminationRequested;
        
        readonly BackgroundJobTokenSource jts = new BackgroundJobTokenSource();
        readonly HashSet<BackgroundJob> tracker;
        readonly ActorRef host;
        readonly ITimerService timers;
        Func<BackgroundJobToken, Task> job;

        JobStatus status;

        protected internal BackgroundJob(HashSet<BackgroundJob> tracker, ActorRef host, ITimerService timers, string name, int id, Func<BackgroundJobToken, Task> job) => 
            (Name, Id, this.tracker, this.host, this.timers, this.job) = (name, id, tracker, host, timers, job);

        protected internal void Schedule(TimeSpan due)
        {
            status = JobStatus.Scheduled;
            tracker.Add(this);

            timers.Register(TimerUid, due, jts.Token, async token =>
            {
                if (token.IsTerminationRequested) // short-circuit
                {
                    status = JobStatus.Terminated;
                    tracker.Remove(this);

                    host.Notify(new BackgroundJobTerminated(Name, Id));
                    return;
                }

                status = JobStatus.Running;
                tracker.Add(this);

                try
                {
                    await job(token);

                    status = JobStatus.Terminated;
                    tracker.Remove(this);

                    host.Notify(new BackgroundJobTerminated(Name, Id));
                    job = null; // allow to GC resources that could be held by the closure
                }
                catch (Exception ex)
                {
                    Failures++;

                    status = JobStatus.Failed;
                    tracker.Remove(this);

                    host.Notify(new BackgroundJobFailed(Name, Id, Failures, ex));
                }
            });
        }

        /// <summary>
        /// The unique job id under which it is registered in timer service
        /// </summary>
        protected string TimerUid => $":job:{Name}[{Id}]";

        /// <summary>
        /// Re-schedules run of the failed job.
        /// </summary>
        /// <param name="due">The optional due time to run the job</param>
        public void Retry(TimeSpan? due = null)
        {
            if (status != JobStatus.Failed)
                throw new InvalidOperationException(
                    "Only faulted jobs could be retried. " +
                    $"The status of '{ToString()}' jobs is: {status}");

            Schedule(due ?? TimeSpan.Zero);
        }

        /// <summary>
        /// Requests job termination. The actual termination of the job
        /// will be signaled back via <see cref="BackgroundJobTerminated"/> message
        /// Has effect only if is job is either scheduled or running.
        /// </summary>
        public void Terminate() => jts.Terminate();

        /// <inheritdoc />
        public override string ToString() => $"{Name}[{Id}]";
        /// <inheritdoc />
        public override bool Equals(object obj) => Id.Equals(Id);
        /// <inheritdoc />
        public override int GetHashCode() => Id.GetHashCode();
    }

    /// <summary>
    /// Useful extension methods for <see cref="IBackgroundJobService"/>
    /// </summary>
    public static class BackgroundJobServiceExtensions
    {
        /// <summary>
        /// If no job <paramref name="id"/> is specified
        /// will terminate all jobs with specified <paramref name="name"/>
        /// </summary>
        /// <param name="service">The job service</param>
        /// <param name="name">The name of the job</param>
        /// <param name="id">The unique id of the job</param>
        public static void Terminate(this IBackgroundJobService service, string name, int? id = null)
        {
            Requires.NotNull(name, nameof(name));

            foreach (var each in service.Active().ToArray())
            {
                if (each.Name != name || id.HasValue && each.Id != id) 
                    continue;

                each.Terminate();
            }
        }
    }
    
    /// <summary>
    /// Signals to the <see cref="BackgroundJobToken"/> that it should be terminated
    /// </summary>
    public class BackgroundJobTokenSource
    {
        /// <summary>
        /// The actual termination token which could be passed to job
        /// </summary>
        public BackgroundJobToken Token { get; } = new BackgroundJobToken();

        /// <summary>
        /// Signals job termination. Could be called multiple times (ie, idempotent)
        /// </summary>
        public void Terminate() => Token.IsTerminationRequested = true;
    }

    /// <summary>
    /// Propagates notification that job should be terminated
    /// </summary>
    public class BackgroundJobToken
    {
        /// <summary>
        /// Answers whether the job termination was requested for this token
        /// </summary>
        public bool IsTerminationRequested { get; internal set; }
    }

    /// <summary>
    /// This message is sent back to the actor hosting the job
    /// when job invocation has been failed
    /// </summary>
    [Immutable] public class BackgroundJobFailed
    {
        /// <summary>
        /// The name of the job
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// The unique id of the job (PID)
        /// </summary>
        public readonly int Id;

        /// <summary>
        /// The number of times this job has failed
        /// </summary>
        public readonly int Failures;

        /// <summary>
        /// The exception that was thrown
        /// </summary>
        public readonly Exception Exception;

        /// <summary>
        /// Creates new instance of <see cref="BackgroundJobFailed"/> message
        /// </summary>
        /// <param name="name">The name of the job</param>
        /// <param name="id">The unique id of the job (PID)</param>
        /// <param name="exception">The exception that was thrown</param>
        /// <param name="failures">The number of times this job has failed</param>
        public BackgroundJobFailed(string name, int id, int failures, Exception exception) => 
            (Name, Id, Failures, Exception) = (name, id, failures, exception);
    }

    /// <summary>
    /// This message is sent back to the actor hosting the job
    /// when job execution has been terminated, either
    /// by itself (ran to completion) or terminated externally.
    /// </summary>
    [Immutable] public class BackgroundJobTerminated
    {
        /// <summary>
        /// The name of the job
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// The unique id of the job (PID)
        /// </summary>
        public readonly int Id;

        /// <summary>
        /// Creates new instance of <see cref="BackgroundJobTerminated"/> message
        /// </summary>
        /// <param name="name">The name of the job</param>
        /// <param name="id">The unique id of the job (PID)</param>
        public BackgroundJobTerminated(string name, int id) => 
            (Name, Id) = (name, id);
    }
}