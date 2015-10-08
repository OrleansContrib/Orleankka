using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Orleans;

namespace Orleankka.TestKit
{
    using Services;

    public class ReminderServiceMock : IReminderService, IEnumerable<RecordedReminder>
    {
        readonly Dictionary<string, RecordedReminder> recorded = new Dictionary<string, RecordedReminder>();

        Task IReminderService.Register(string id, TimeSpan due, TimeSpan period)
        {
            recorded.Add(id, new RecordedReminder(id, due, period));
            return TaskDone.Done;
        }

        Task IReminderService.Unregister(string id)
        {
            recorded.Remove(id);
            return TaskDone.Done;
        }

        Task<bool> IReminderService.IsRegistered(string id) => Task.FromResult(recorded.ContainsKey(id));
        Task<IEnumerable<string>> IReminderService.Registered() => Task.FromResult(recorded.Keys.AsEnumerable());

        public IEnumerator<RecordedReminder> GetEnumerator() => recorded.Values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public RecordedReminder this[int index] => recorded.Values.ElementAt(index);
        public RecordedReminder this[string id] => recorded[id];

        public void Clear() => recorded.Clear();
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
