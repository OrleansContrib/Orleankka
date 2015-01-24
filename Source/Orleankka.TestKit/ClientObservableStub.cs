using System;
using System.Linq;

namespace Orleankka.TestKit
{
    public class ClientObservableStub : ClientObservable
    {
        protected ClientObservableStub()
            : base(new ActorObserverPath(Guid.NewGuid().ToString("D")))
        {}

        public override IDisposable Subscribe(IObserver<Notification> observer)
        {
            return new DisposableStub();
        }

        class DisposableStub : IDisposable
        {
            public void Dispose()
            {}
        }

        public override void Dispose()
        {}
    }
}
