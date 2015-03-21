using System;
using System.Linq;

using Orleankka;

namespace Example.Native.Serialization
{
    [Serializable] 
    public class GetLevel 
    {}

    [Serializable]
    public class GetManager
    {}

    [Serializable]
    public class SetManager
    {
        public ActorRef Manager;
    }

    [Serializable]
    public class Promote
    {
        public int NewLevel;
    }

    [Serializable]
    public class Greeting
    {
        public ActorRef From;
        public string Text;
    }
}
