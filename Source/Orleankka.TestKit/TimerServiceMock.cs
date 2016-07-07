using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orleankka.TestKit
{
    using Services;

    public class TimerServiceMock : ITimerService, IEnumerable<RecordedTimer>
    {
        readonly Dictionary<string, RecordedTimer> timers = new Dictionary<string, RecordedTimer>();

        void ITimerService.Register(string id, TimeSpan due, TimeSpan period, object state)
        {
            timers.Add(id, new RecordedTimer<object>(id, due, period, null, null));
        }

        void ITimerService.Register(string id, TimeSpan due, TimeSpan period, Func<Task> callback)
        {
            timers.Add(id, new RecordedTimer(id, due, period, callback));
        }

        void ITimerService.Register<TState>(string id, TimeSpan due, TimeSpan period, TState state, Func<TState, Task> callback)
        {
            timers.Add(id, new RecordedTimer<TState>(id, due, period, callback, state));
        }

        void ITimerService.Unregister(string id) => timers.Remove(id);
        bool ITimerService.IsRegistered(string id) => timers.ContainsKey(id);

        IEnumerable<string> ITimerService.Registered() => timers.Keys;

        public IEnumerator<RecordedTimer> GetEnumerator() => timers.Values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public RecordedTimer this[int index] => timers.Values.ElementAt(index);
        public RecordedTimer this[string id] => timers[id];

        public void Reset() => timers.Clear();
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
