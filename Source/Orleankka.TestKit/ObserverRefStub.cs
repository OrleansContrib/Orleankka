using System;
using System.Linq;

namespace Orleankka.TestKit
{
    public class ObserverRefStub : ObserverRef
    {
        public ObserverRefStub(string path)
            : base(path)
        {}

        public override void Notify(Notification notification)
        {}
    }
}