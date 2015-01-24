using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orleankka.TestKit
{
    public class TimerServiceMock : ITimerService, IEnumerable<RecordedTimer>
    {
        readonly List<RecordedTimer> recorded = new List<RecordedTimer>();

        void ITimerService.Register(string id, TimeSpan due, TimeSpan period, Func<Task> callback)
        {
            recorded.Add(new RecordedTimer(id, due, period, callback));
        }

        void ITimerService.Register<TState>(string id, TimeSpan due, TimeSpan period, TState state, Func<TState, Task> callback)
        {
            recorded.Add(new RecordedTimer<TState>(id, due, period, callback, state));
        }

        void ITimerService.Unregister(string id)
        {
            recorded.RemoveAll(x => x.Id == id);
        }

        bool ITimerService.IsRegistered(string id)
        {
            return recorded.Exists(x => x.Id == id);
        }

        IEnumerable<string> ITimerService.Registered()
        {
            return recorded.Select(x => x.Id);
        }

        public IEnumerator<RecordedTimer> GetEnumerator()
        {
            return recorded.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public RecordedTimer this[int index]
        {
            get { return recorded.ElementAt(index); }
        }

        public RecordedTimer this[string id]
        {
            get { return recorded.SingleOrDefault(x => x.Id == id); }
        }

        public void Clear()
        {
            recorded.Clear();
        }
    }

    public class RecordedTimer
    {
        public readonly string Id;
        public readonly TimeSpan Due;
        public readonly TimeSpan Period;
        public readonly Func<Task> Callback;

        public RecordedTimer(string id, TimeSpan due, TimeSpan period, Func<Task> callback)
        {
            Id = id;
            Due = due;
            Period = period;
            Callback = callback;
        }
    }

    public class RecordedTimer<TState> : RecordedTimer
    {
        new public readonly Func<TState, Task> Callback;
        public readonly TState State;

        public RecordedTimer(string id, TimeSpan due, TimeSpan period, Func<TState, Task> callback, TState state)
            : base(id, due, period, null)
        {
            Callback = callback;
            State = state;
        }
    }
}
