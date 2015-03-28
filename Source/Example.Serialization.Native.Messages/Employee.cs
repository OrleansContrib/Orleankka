using System;
using System.Linq;

using Orleankka;
using Orleankka.Meta;

namespace Example.Native.Serialization
{
    [Serializable]
    public class Promote
    {
        public int NewLevel;
    }

    [Serializable] 
    public class GetLevel : Query<int>
    {}

    [Serializable]
    public class SetManager : Command
    {
        public ActorRef Manager;
    }

    [Serializable]
    public class GetManager : Query<ActorRef>
    {}

    [Serializable]
    public class Greeting : Command
    {
        public ActorRef From;
        public string Text;
    }
}
