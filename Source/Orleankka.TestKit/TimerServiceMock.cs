using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orleankka.TestKit
{
    using Services;
    using Utility;

    public class TimerServiceMock : ITimerService, IEnumerable<RecordedTimer>
    {
        readonly Dictionary<string, RecordedTimer> timers = new Dictionary<string, RecordedTimer>();
        readonly List<RecordedTimerRequest> requests = new List<RecordedTimerRequest>();

        void ITimerService.Register(string id, TimeSpan due, Func<Task> callback)
        {
            RecordRegister(id, new RecordedTimer(id, due, TimeSpan.Zero, callback));
        }

        void ITimerService.Register(string id, TimeSpan due, TimeSpan period, Func<Task> callback)
        {
            CheckGreaterThanZero(period);

            RecordRegister(id, new RecordedTimer(id, due, period, callback));
        }

        void ITimerService.Register<TState>(string id, TimeSpan due, TState state, Func<TState, Task> callback)
        {
            RecordRegister(id, new RecordedTimer<TState>(id, due, TimeSpan.Zero, callback, state));
        }

        void ITimerService.Register<TState>(string id, TimeSpan due, TimeSpan period, TState state, Func<TState, Task> callback)
        {
            CheckGreaterThanZero(period);

            RecordRegister(id, new RecordedTimer<TState>(id, due, period, callback, state));
        }

        void RecordRegister(string id, RecordedTimer timer)
        {
            timers.Add(id, timer);
            requests.Add(new RecordedTimerRequest(id, RecorderTimerRequestKind.Register, timer));
        }

        void RecordUnregister(string id)
        {
            var registered = timers.Find(id);
            if (registered == null)
                throw new InvalidOperationException($"Timer with id '{id}' has not been registered");

            timers.Remove(id);
            requests.Add(new RecordedTimerRequest(id, RecorderTimerRequestKind.Unregister, registered));
        }

        void CheckGreaterThanZero(TimeSpan period)
        {
            if (period <= TimeSpan.Zero)
                throw new ArgumentException("period should be greater than zero", nameof(period));
        }

        void ITimerService.Unregister(string id) => RecordUnregister(id);
        public bool IsRegistered(string id) => timers.ContainsKey(id);
        public bool IsRegistered(Func<Task> callback) => IsRegistered(callback.Method.Name);

        public IEnumerable<string> Registered() => timers.Keys;
        public RecordedTimerRequest[] Requests => requests.ToArray(); 

        public IEnumerator<RecordedTimer> GetEnumerator() => timers.Values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public RecordedTimer this[int index] => timers.Values.ElementAt(index);
        public RecordedTimer this[string id] => timers[id];

        public void Reset()
        {
            timers.Clear();
            requests.Clear();
        }
    }

    public class RecordedTimerRequest
    {
        public readonly string Id;
        public readonly RecorderTimerRequestKind Kind;
        public RecordedTimer Timer;

        internal RecordedTimerRequest(string id, RecorderTimerRequestKind kind, RecordedTimer timer)
        {
            Id = id;
            Kind = kind;
            Timer = timer;
        }
    }

    public enum RecorderTimerRequestKind
    {
        Register,
        Unregister
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

        public bool IsOneOff => Period == TimeSpan.Zero;
    }

    public class RecordedTimer<TState> : RecordedTimer
    {
        public new readonly Func<TState, Task> Callback;
        public readonly TState State;

        public RecordedTimer(string id, TimeSpan due, TimeSpan period, Func<TState, Task> callback, TState state)
            : base(id, due, period, null)
        {
            Callback = callback;
            State = state;
        }
    }
}
