using System;

namespace Orleankka.TestKit
{
    public class ClientObservableMock : IClientObservable
    {
        public ObserverRef Ref => new ClientRef("path");

        public IDisposable Subscribe(IObserver<object> observer)
        {
            return new DisposableStub();
        }

        class DisposableStub : IDisposable
        {
            public void Dispose()
            {}
        }

        public void Dispose()
        {}
    }
}
