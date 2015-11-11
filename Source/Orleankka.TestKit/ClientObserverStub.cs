using System;
using System.Linq;

namespace Orleankka.TestKit
{
    public class ClientObserverStub : ClientObserver
    {
        protected ClientObserverStub()
            : base(new ObserverRefStub())
        {}

        public override IDisposable Subscribe(IObserver<object> observer)
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
