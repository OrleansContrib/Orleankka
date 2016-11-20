using System;
using System.Web;
using System.Threading.Tasks;
using System.Runtime.Serialization;

using Orleankka;
using Orleankka.Meta;

namespace Demo
{
    [Serializable]
    public class Search : Query<int>
    {
        public readonly string Subject;

        public Search(string subject)
        {
            Subject = subject;
        }
    }

    [Serializable]
    public class Subscribe : Command
    {
        public readonly ObserverRef Observer;

        public Subscribe(ObserverRef observer)
        {
            Observer = observer;
        }
    }

    [Serializable]
    public class AvailabilityChanged : Event
    {
        public readonly ActorRef Api;
        public readonly bool Available;

        public AvailabilityChanged(ActorRef api, bool available)
        {
            Api = api;
            Available = available;
        }
    }

    [Serializable]
    public class ApiUnavailableException : ApplicationException
    {
        public ApiUnavailableException(string api)
            : base(api + " api is unavailable. Try later!")
        {}

        protected ApiUnavailableException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {}
    }

    [ActorType("Api")]
    public class Api : Actor
    {
        const int FailureThreshold = 3;

        readonly IObserverCollection observers;
        readonly Func<IApiWorker> worker;

        int failures;
        bool available = true;

        public Api()
        {
            observers = new ObserverCollection();
            worker = ApiWorkerFactory.Create(()=> Id);
        }

        public Api(
            string id, 
            IActorRuntime runtime, 
            IObserverCollection observers, 
            IApiWorker worker)
            : base(id, runtime)
        {
            this.observers = observers;
            this.worker = ()=> worker;
        }
    
        public void Handle(Subscribe cmd)
        {
            observers.Add(cmd.Observer);
        }

        public async Task<int> Handle(Search search)
        {
            if (!available)
                throw new ApiUnavailableException(Id);

            try
            {
                var result = await worker().Search(search.Subject);
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
                await worker().Search("test");
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
