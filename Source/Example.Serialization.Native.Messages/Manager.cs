using System;
using System.Linq;

using Orleankka;

namespace Example.Native.Serialization
{
    [Serializable]
    public class GetDirectReports
    {}

    [Serializable]
    public class AddDirectReport
    {
        public ActorRef Employee;
    }
}
