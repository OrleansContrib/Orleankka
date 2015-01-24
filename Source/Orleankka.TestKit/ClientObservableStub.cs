using System;
using System.Linq;

namespace Orleankka.TestKit
{
    public class ClientObservableStub : IClientObservable
    {
        readonly IActorObserver proxy = new ProxyStub();

        public IActorObserver Proxy
        {
            get {return proxy;}
        }

        class ProxyStub : IActorObserver
        {
            public void OnNext(Notification notification)
            {}
        }

        public IDisposable Subscribe(IObserver<Notification> observer)
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
