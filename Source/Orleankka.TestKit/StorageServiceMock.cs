using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orleankka.TestKit
{
    using Services;

    public class StorageServiceMock<TState> : IStorageService<TState>, IEnumerable<RecordedStorageRequest> where TState : new()
    {
        readonly List<RecordedStorageRequest> requests = new List<RecordedStorageRequest>();

        public StorageServiceMock()
        {}

        public StorageServiceMock(TState initial)
        {
            State = initial;
        }

        public TState State
        {
            get; set;
        }

        Task IStorageService<TState>.ReadState()
        {
            requests.Add(new ReadStateRequest());
            return Task.CompletedTask;
        }

        Task IStorageService<TState>.WriteState()
        {
            requests.Add(new WriteStateRequest());
            return Task.CompletedTask;
        }

        Task IStorageService<TState>.ClearState()
        {
            requests.Add(new ClearStateRequest());
            return Task.CompletedTask;
        }

        public IEnumerator<RecordedStorageRequest> GetEnumerator() => requests.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public RecordedStorageRequest[] Requests => requests.ToArray();
        public RecordedStorageRequest this[int index] => requests.ElementAt(index);

        public void Reset() => requests.Clear();
    }

    public abstract class RecordedStorageRequest
    {
        public virtual bool IsReadState => false;
        public virtual bool IsWriteState => false;
        public virtual bool IsClearState => false;
    }

    public class ReadStateRequest : RecordedStorageRequest
    {
        public override bool IsReadState => true;
    }

    public class WriteStateRequest : RecordedStorageRequest
    {
        public override bool IsWriteState => true;
    }

    public class ClearStateRequest : RecordedStorageRequest
    {
        public override bool IsClearState => true;
    }
}
