using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Orleankka.Services;

using Orleans;

namespace Orleankka.TestKit
{
    public class ReminderServiceMock : IReminderService, IEnumerable<RecordedReminder>
    {
        readonly List<RecordedReminder> recorded = new List<RecordedReminder>();

        Task IReminderService.Register(string id, TimeSpan due, TimeSpan period)
        {
            recorded.Add(new RecordedReminder(id, due, period));
            return TaskDone.Done;
        }

        Task IReminderService.Unregister(string id)
        {
            recorded.RemoveAll(x => x.Id == id);
            return TaskDone.Done;
        }

        Task<bool> IReminderService.IsRegistered(string id)
        {
            return Task.FromResult(recorded.Exists(x => x.Id == id));
        }

        Task<IEnumerable<string>> IReminderService.Registered()
        {
            return Task.FromResult(recorded.Select(x => x.Id));
        }

        public IEnumerator<RecordedReminder> GetEnumerator()
        {
            return recorded.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public RecordedReminder this[int index]
        {
            get { return recorded.ElementAt(index); }
        }

        public RecordedReminder this[string id]
        {
            get { return recorded.SingleOrDefault(x => x.Id == id); }
        }

        public void Clear()
        {
            recorded.Clear();
        }
    }

    public class RecordedReminder
    {
        public readonly string Id;
        public readonly TimeSpan Due;
        public readonly TimeSpan Period;

        public RecordedReminder(string id, TimeSpan due, TimeSpan period)
        {
            Id = id;
            Due = due;
            Period = period;
        }
    }
}
