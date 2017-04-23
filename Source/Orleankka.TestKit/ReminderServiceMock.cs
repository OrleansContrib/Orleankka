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
        readonly List<RecordedReminderRequest> requests = new List<RecordedReminderRequest>();

        Task IReminderService.Register(string id, TimeSpan due, TimeSpan period)
        {
            var reminder = new RecordedReminder(id, due, period);
            var request = new RecordedReminderRequest(id, RecordedReminderRequestKind.Register, reminder);
            reminders.Add(id, reminder);
            requests.Add(request);
            return TaskDone.Done;
        }

        Task IReminderService.Unregister(string id)
        {
            reminders.Remove(id);
            var request = new RecordedReminderRequest(id, RecordedReminderRequestKind.Unregister, null);
            requests.Add(request);
            return TaskDone.Done;
        }

        public Task<bool> IsRegistered(string id) => Task.FromResult(reminders.ContainsKey(id));
        public Task<IEnumerable<string>> Registered() => Task.FromResult(reminders.Keys.AsEnumerable());

        public IEnumerator<RecordedReminder> GetEnumerator() => reminders.Values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public RecordedReminder this[int index] => reminders.Values.ElementAt(index);
        public RecordedReminder this[string id] => reminders[id];

        public RecordedReminderRequest[] Requests => requests.ToArray();

        public void Reset()
        {
            reminders.Clear();
            requests.Clear();
        }
    }

    public class RecordedReminderRequest
    {
        public readonly string Id;
        public readonly RecordedReminderRequestKind Kind;
        public RecordedReminder Reminder;

        internal RecordedReminderRequest(string id, RecordedReminderRequestKind kind, RecordedReminder reminder)
        {
            Id = id;
            Kind = kind;
            Reminder = reminder;
        }
    }

    public enum RecordedReminderRequestKind
    {
        Register,
        Unregister
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
