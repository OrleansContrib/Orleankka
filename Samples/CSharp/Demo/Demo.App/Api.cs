using System;
using System.Threading.Tasks;
using System.Runtime.Serialization;

using Orleankka;
using Orleankka.Meta;

using Orleans;

namespace Demo
{
    [Serializable, GenerateSerializer]
    public class Search : Query<int>
    {
        [Id(0)] public readonly string Subject;

        public Search(string subject)
        {
            Subject = subject;
        }
    }

    [Serializable, GenerateSerializer]
    public class Subscribe : Command
    {
        [Id(0)] public readonly ObserverRef Observer;

        public Subscribe(ObserverRef observer)
        {
            Observer = observer;
        }
    }

    [Serializable, GenerateSerializer]
    public class AvailabilityChanged : Event
    {
        [Id(0)] public readonly ActorRef Api;
        [Id(1)] public readonly bool Available;

        public AvailabilityChanged(ActorRef api, bool available)
        {
            Api = api;
            Available = available;
        }
    }

    [Serializable, GenerateSerializer]
    public class ApiUnavailableException : ApplicationException
    {
        public ApiUnavailableException(string api)
            : base(api + " api is unavailable. Try later!")
        {}
    }

    public interface IApi : IActorGrain, IGrainWithStringKey
    {}
    
    public class Api : DispatchActorGrain, IApi
    {
        const int FailureThreshold = 3;

        IObserverCollection observers;
        IApiWorker worker;

        int failures;
        bool available = true;

        public Api( 
            IApiWorker worker = null,
            IObserverCollection observers = null,
            IActorRuntime runtime = null)
            : base(runtime)
        {
            this.worker = worker;
            this.observers = observers;
        }

        void On(Activate _)
        {
            observers = observers ?? new ObserverCollection();
            worker = worker ?? ApiWorkerFactory.Create(Id);
        }

        public void Handle(Subscribe cmd) => observers.Add(cmd.Observer);

        public async Task<int> Handle(Search search)
        {
            if (!available)
                throw new ApiUnavailableException(Id);

            try
            {
                var result = await worker.Search(search.Subject);
                ResetFailureCounter();

                return result;
            }
            catch (HttpException)
            {
                IncrementFailureCounter();
                
                if (!HasReachedFailureThreshold())
                    throw new ApiUnavailableException(Id);

                Lock();
                Notify();

                ScheduleAvailabilityCheck();
                throw new ApiUnavailableException(Id);
            }
        }

        bool HasReachedFailureThreshold()   => failures == FailureThreshold;
        void IncrementFailureCounter()      => failures++;
        void ResetFailureCounter()          => failures = 0;

        void ScheduleAvailabilityCheck()
        {
            var due = TimeSpan.FromSeconds(1);
            var period = TimeSpan.FromSeconds(1);

            Timers.Register("check", due, period, CheckAvailability);
        }

        public async Task CheckAvailability()
        {
            try
            {
                await worker.Search("test");
                Timers.Unregister("check");

                Unlock();
                Notify();
            }
            catch (HttpException)
            {}
        }

        void Lock()   => available = false;
        void Unlock() => available = true;
        void Notify() => observers.Notify(new AvailabilityChanged(Self, available));
    }
}
