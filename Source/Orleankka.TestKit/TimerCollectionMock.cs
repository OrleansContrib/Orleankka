using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orleankka.TestKit
{
    public class TimerCollectionMock : ITimerCollection, IEnumerable<RecordedTimer>
    {
        readonly List<RecordedTimer> recorded = new List<RecordedTimer>();

        void ITimerCollection.RegisterReentrant(string id, TimeSpan due, TimeSpan period, Func<Task> callback)
        {
            recorded.Add(new RecordedReentrantTimer(id, due, period, callback));
        }

        void ITimerCollection.RegisterReentrant<TState>(string id, TimeSpan due, TimeSpan period, TState state, Func<TState, Task> callback)
        {
            recorded.Add(new RecordedReentrantTimer<TState>(id, due, period, state, callback));
        }

        public void Register(string id, TimeSpan due, TimeSpan period, object state)
        {
            recorded.Add(new RecordedNonReentrantTimer(id, due, period, state));
        }

        void ITimerCollection.Unregister(string id)
        {
            recorded.RemoveAll(x => x.Id == id);
        }

        bool ITimerCollection.IsRegistered(string id)
        {
            return recorded.Exists(x => x.Id == id);
        }

        IEnumerable<string> ITimerCollection.Registered()
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

    public abstract class RecordedTimer
    {
        public readonly string Id;
        public readonly TimeSpan Due;
        public readonly TimeSpan Period;

        public RecordedTimer(string id, TimeSpan due, TimeSpan period)
        {
            Id = id;
            Due = due;
            Period = period;
        }
    }
    
    public class RecordedReentrantTimer: RecordedTimer
    {
        public readonly Func<Task> Callback;

        public RecordedReentrantTimer(string id, TimeSpan due, TimeSpan period, Func<Task> callback)
            : base(id, due, period)
        {
            Callback = callback;
        }
    }

    public class RecordedReentrantTimer<TState> : RecordedTimer
    {
        public readonly Func<TState, Task> Callback;
        public readonly TState State;

        public RecordedReentrantTimer(string id, TimeSpan due, TimeSpan period, TState state, Func<TState, Task> callback)
            : base(id, due, period)
        {
            Callback = callback;
            State = state;
        }
    }

    public class RecordedNonReentrantTimer : RecordedTimer
    {
        public readonly object State;

        public RecordedNonReentrantTimer(string id, TimeSpan due, TimeSpan period, object state)
            : base(id, due, period)
        {
            State = state;
        }
    }
}
