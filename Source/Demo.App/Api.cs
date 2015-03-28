using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Web;

using Orleankka;
using Orleankka.Meta;
using Orleankka.Services;

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

    public class Api : Actor
    {
        const int FailureThreshold = 3;

        readonly ITimerService timers;
        readonly IObserverCollection observers;
        readonly Func<IApiWorker> worker;

        int failures;
        bool available = true;

        public Api()
        {
            timers = new TimerService(this);
            observers = new ObserverCollection();
            worker = ApiWorkerFactory.Create(()=> Id);
        }

        public Api(
            string id, 
            IActorSystem system, 
            ITimerService timers, 
            IObserverCollection observers, 
            IApiWorker worker)
            : base(id, system)
        {
            this.timers = timers;
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

                NotifyUnavailable();
                ScheduleAvailabilityCheck();

                throw new ApiUnavailableException(Id);
            }
        }

        bool HasReachedFailureThreshold()
        {
            return failures == FailureThreshold;
        }

        void IncrementFailureCounter()
        {
            failures++;
        }

        void ResetFailureCounter()
        {
            failures = 0;
        }

        void ScheduleAvailabilityCheck()
        {
            var due = TimeSpan.FromSeconds(1);
            var period = TimeSpan.FromSeconds(1);

            timers.Register("check", due, period, CheckAvailability);
        }

        public async Task CheckAvailability()
        {
            try
            {
                await worker().Search("test");
                timers.Unregister("check");

                Unlock();
                NotifyAvailable();
            }
            catch (HttpException)
            {}
        }

        void Lock()
        {
            available = false;            
        }

        void Unlock()
        {
            available = true;
        }

        void NotifyAvailable()
        {
            observers.Notify(new AvailabilityChanged(Self, true));
        }

        void NotifyUnavailable()
        {
            observers.Notify(new AvailabilityChanged(Self, false));
        }
    }
}
