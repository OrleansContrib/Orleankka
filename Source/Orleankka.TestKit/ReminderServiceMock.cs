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
        readonly Dictionary<string, RecordedReminder> reminders = new Dictionary<string, RecordedReminder>();

        Task IReminderService.Register(string id, TimeSpan due, TimeSpan period)
        {
            reminders.Add(id, new RecordedReminder(id, due, period));
            return TaskDone.Done;
        }

        Task IReminderService.Unregister(string id)
        {
            reminders.Remove(id);
            return TaskDone.Done;
        }

        Task<bool> IReminderService.IsRegistered(string id) => Task.FromResult(reminders.ContainsKey(id));
        Task<IEnumerable<string>> IReminderService.Registered() => Task.FromResult(reminders.Keys.AsEnumerable());

        public IEnumerator<RecordedReminder> GetEnumerator() => reminders.Values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public RecordedReminder this[int index] => reminders.Values.ElementAt(index);
        public RecordedReminder this[string id] => reminders[id];

        public void Reset() => reminders.Clear();
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
