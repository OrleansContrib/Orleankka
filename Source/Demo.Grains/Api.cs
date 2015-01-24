using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

using Orleankka;

using Orleans;

namespace Demo
{
    public class ApiActor : Actor, IApi
    {
        Api api;

        public override Task ActivateAsync()
        {
            api = new Api
            {
                Id = Id,
                Timers = new TimerService(this),
                Observers = new ActorObserverCollection(()=> Path),
                Worker = new FaultyDemoWorker(Id)  // ApiWorkerFactory.Create(Id())
            };

            return Task.FromResult(api);
        }

        public override Task OnTell(object message)
        {
            return api.Handle((dynamic)message);
        }

        public override async Task<object> OnAsk(object message)
        {
            return await api.Answer((dynamic)message);
        }
    }

    public class Api
    {
        const int FailureThreshold = 3;

        public string Id;
        public ITimerService Timers;
        public IActorObserverCollection Observers;
        public IApiWorker Worker;

        int failures;
        bool available = true;

        public Task Handle(MonitorAvailabilityChanges cmd)
        {
            Observers.Add(cmd.Observer);
            return TaskDone.Done;
        }

        public async Task<int> Answer(Search search)
        {
            if (!available)
                throw new ApiUnavailableException(Id);

            try
            {
                var result = await Worker.Search(search.Subject);
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

            Timers.Register("check", due, period, CheckAvailability);
        }

        public async Task CheckAvailability()
        {
            try
            {
                await Worker.Search("test");
                Timers.Unregister("check");

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
            Observers.Notify(new AvailabilityChanged(Id, true));
        }

        void NotifyUnavailable()
        {
            Observers.Notify(new AvailabilityChanged(Id, false));
        }
    }
}
