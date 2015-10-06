using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Threading.Tasks;

using Orleans;
using Orleans.Streams;

namespace Orleankka
{
    using Utility;

    [Serializable]
    [DebuggerDisplay("s->{ToString()}")]
    public class StreamRef : Ref, IEquatable<StreamRef>, IEquatable<StreamPath>, ISerializable
    {
        public static StreamRef Deserialize(StreamPath path)
        {            
            return new StreamRef(path);
        }

        protected internal StreamRef(StreamPath path)
        {
            Path = path;
        }

        IAsyncStream<object> endpoint;
        IAsyncStream<object> Endpoint => endpoint ?? (endpoint = Path.Proxy());

        public StreamPath Path { get; }
        public override string Serialize() => Path.Serialize();

        public virtual Task Push(object item)
        {
            return Endpoint.OnNextAsync(item);
        }

        public virtual async Task<StreamSubscription> Subscribe(Func<StreamPath, object, Task> callback)
        {
            Requires.NotNull(callback, nameof(callback));

            var observer = new Observer((item, token) => callback(Path, item));
            var handle = await Endpoint.SubscribeAsync(observer);

            return new StreamSubscription(handle);
        }

        public virtual async Task Subscribe(Actor actor)
        {
            Requires.NotNull(actor, nameof(actor));

            var handles = await GetAllSubscriptionHandles();
            if (handles.Count == 1)
                return;

            Debug.Assert(handles.Count == 0,
                "We should keep only one active subscription per-stream per-actor");

            var observer = new Observer((item, token) => actor.OnReceive(item));
            await Endpoint.SubscribeAsync(observer);
        }

        public async Task Unsubscribe(Actor actor)
        {
            Requires.NotNull(actor, nameof(actor));

            var handles = await GetAllSubscriptionHandles();
            if (handles.Count == 0)
                return;

            Debug.Assert(handles.Count == 1, 
                "We should keep only one active subscription per-stream per-actor");

            await handles[0].UnsubscribeAsync();
        }

        public async Task Resume(Actor actor)
        {
            Requires.NotNull(actor, nameof(actor));

            var handles = await GetAllSubscriptionHandles();
            if (handles.Count == 0)
                return;

            Debug.Assert(handles.Count == 1,
                "We should keep only one active subscription per-stream per-actor");

            var observer = new Observer((item, token) => actor.OnReceive(item));
            await handles[0].ResumeAsync(observer);
        }

        internal Task<IList<StreamSubscriptionHandle<object>>> GetAllSubscriptionHandles()
        {
            return Endpoint.GetAllSubscriptionHandles();
        }

        public bool Equals(StreamRef other)
        {
            return !ReferenceEquals(null, other) && (ReferenceEquals(this, other)
                    || Path.Equals(other.Path));
        }

        public override bool Equals(object obj)
        {
            return !ReferenceEquals(null, obj) && (ReferenceEquals(this, obj)
                    || obj.GetType() == GetType() && Equals((StreamRef)obj));
        }

        public bool Equals(StreamPath other) => Path.Equals(other);
        public override int GetHashCode() => Path.GetHashCode();

        public static bool operator ==(StreamRef left, StreamRef right) => Equals(left, right);
        public static bool operator !=(StreamRef left, StreamRef right) => !Equals(left, right);

        public override string ToString() => Path.ToString();

        #region Default Binary Serialization

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("path", Serialize(), typeof(string));
        }

        public StreamRef(SerializationInfo info, StreamingContext context)
        {
            var value = (string)info.GetValue("path", typeof(string));
            Path = StreamPath.Deserialize(value);
        }

        #endregion

        class Observer : IAsyncObserver<object>
        {
            readonly Func<object, StreamSequenceToken, Task> callback;

            public Observer(Func<object, StreamSequenceToken, Task> callback)
            {
                this.callback = callback;
            }

            public Task OnNextAsync(object item, StreamSequenceToken token = null) 
                => callback(item, token);

            public Task OnCompletedAsync()           => TaskDone.Done;
            public Task OnErrorAsync(Exception ex)   => TaskDone.Done;
        }
    }
}