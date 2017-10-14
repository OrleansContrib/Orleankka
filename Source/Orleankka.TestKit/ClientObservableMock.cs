using System;
using System.Collections.Generic;

namespace Orleankka.TestKit
{
    using Utility;

    public class ClientObservableMock : IClientObservable
    {
        readonly List<IObserver<object>> subscribers = 
             new List<IObserver<object>>();

        public IObserver<object>[] Subscribers => subscribers.ToArray();

        public ClientRef Ref { get; } = new ClientRef(Guid.NewGuid().ToString("N"));

        public IDisposable Subscribe(IObserver<object> observer)
        {
            Requires.NotNull(observer, nameof(observer));
            return new DisposableStub(subscribers, observer);
        }

        class DisposableStub : IDisposable
        {
            readonly List<IObserver<object>> subscribers;
            readonly IObserver<object> observer;

            public DisposableStub(List<IObserver<object>> subscribers, IObserver<object> observer)
            {
                this.subscribers = subscribers;
                this.observer = observer;
                subscribers.Add(observer);
            }

            public void Dispose() => subscribers.Remove(observer);
        }

        public bool Disposed { get; private set; }

        public void Dispose() => Disposed = true;
    }
}
