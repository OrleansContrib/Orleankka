using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Orleankka;

namespace Demo
{
    public class TopicActor : Actor, ITopic
    {
        Topic topic;

        public override Task ActivateAsync()
        {
            topic = new Topic
            {
                Id = Id,
                System = ActorSystem.Instance,
                Timers = new TimerService(this),
                Reminders = new ReminderService(this),
                Storage = TopicStorage.Instance
            };

            return topic.Activate();
        }

        public override Task OnTell(object message)
        {
            return topic.Handle((dynamic)message);
        }

        public override Task OnReminder(string id)
        {
            return topic.OnReminder(id);
        }
    }

    public class Topic
    {
        public string Id;
        public IActorSystem System;
        public ITimerService Timers;
        public IReminderService Reminders;
        public ITopicStorage Storage;
        public TopicState State;

        const int MaxRetries = 3;
        static readonly TimeSpan RetryPeriod = TimeSpan.FromSeconds(5);
        readonly IDictionary<string, int> retrying = new Dictionary<string, int>();

        string query;

        public async Task Activate()
        {
            State = await Storage.ReadStateAsync(Id);
        }

        public async Task Handle(CreateTopic cmd)
        {
            query = cmd.Query;

            foreach (var entry in cmd.Schedule)
                await Reminders.Register(entry.Key, TimeSpan.Zero, entry.Value);
        }

        public async Task OnReminder(string api)
        {
            try
            {
                if (!IsRetrying(api))
                    await Search(api);
            }
            catch (ApiUnavailableException)
            {
                ScheduleRetries(api);
            }
        }

        bool IsRetrying(string api)
        {
            return retrying.ContainsKey(api);
        }

        public void ScheduleRetries(string api)
        {
            retrying.Add(api, 0);
            Timers.Register(api, RetryPeriod, RetryPeriod, api, RetrySearch);
        }

        public async Task RetrySearch(object state)
        {
            var api = (string)state;
            
            try
            {
                await Search(api);
                CancelRetries(api);
            }
            catch (ApiUnavailableException)
            {
                RecordFailedRetry(api);

                if (MaxRetriesReached(api))
                {
                    DisableSearch(api);
                    CancelRetries(api);                   
                }
            }
        }

        void RecordFailedRetry(string api)
        {
            Log.Message(ConsoleColor.DarkRed, "[{0}] failed to obtain results from {1} ...", Id, api);
            retrying[api] += 1;
        }

        bool MaxRetriesReached(string api)
        {
            return retrying[api] == MaxRetries;
        }

        void CancelRetries(string api)
        {
            Timers.Unregister(api);
            retrying.Remove(api);
        }

        async Task Search(string api)
        {
            var provider = System.ActorOf<IApi>(api);

            State.Total += await provider.Query(new Search(query));
            Log.Message(ConsoleColor.DarkGray, "[{0}] succesfully obtained results from {1} ...", Id, api);

            await Storage.WriteStateAsync(Id, State);
        }

        void DisableSearch(string api)
        {
            Reminders.Unregister(api);
        }
    }

    public class TopicState
    {
        public int Total { get; set; }
    }
}
