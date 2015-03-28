using System;
using System.Collections.Generic;
using System.Linq;

using Orleankka;
using Orleankka.Meta;

namespace Example.Native.Serialization
{
    [Serializable]
    public class AddDirectReport : Command
    {
        public ActorRef Employee;
    }

    [Serializable]
    public class GetDirectReports : Query<IEnumerable<ActorRef>>
    {}
}
