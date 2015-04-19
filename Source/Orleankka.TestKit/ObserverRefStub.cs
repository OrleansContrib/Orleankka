using System;
using System.Linq;

namespace Orleankka.TestKit
{
    public class ObserverRefStub : ObserverRef
    {
        public override void Notify(object message)
        {}

        public override string Serialize()
        {
            throw new NotImplementedException();
        }
    }
}