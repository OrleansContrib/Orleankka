using System;
using System.Linq;

using Orleankka.Client;

namespace Orleankka.TestKit
{
    public class ObserverStub : Observer
    {
        protected ObserverStub()
            : base(new ObserverRefStub(ObserverPath.Parse(Guid.NewGuid().ToString("D"))))
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
