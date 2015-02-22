using System;
using System.Linq;

namespace Orleankka.TestKit
{
    public class ObserverRefStub : ObserverRef
    {
        public override void Notify(object message)
        {}
    }
}