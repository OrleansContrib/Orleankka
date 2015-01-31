using System;
using System.Linq;

namespace Orleankka.TestKit
{
    public class ObserverRefStub : ObserverRef
    {
        public ObserverRefStub(ObserverPath path)
            : base(path)
        {}

        public override void Notify(Notification notification)
        {}
    }
}