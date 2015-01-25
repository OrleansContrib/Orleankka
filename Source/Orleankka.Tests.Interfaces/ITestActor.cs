using System;
using System.Linq;

using Orleans.Concurrency;

namespace Orleankka
{
    [Immutable, Serializable]
    public class DoFoo : Command
    {
        public string Text;
    }

    [Immutable, Serializable]
    public class DoBar : Command
    {
        public string Text;
    }

    [Immutable, Serializable]
    public class GetFoo : Query<string> 
    {}

    [Immutable, Serializable]
    public class GetBar : Query<string> 
    {}

    [Immutable, Serializable]
    public class Throw : Command
    {
        public Exception Exception;
    }

    [Immutable, Serializable]
    public class PublishFoo : Command
    {
        public string Foo;
    }

    [Immutable, Serializable]
    public class FooPublished : Event
    {
        public string Foo;
    }

    [Immutable, Serializable]
    public class PublishBar : Command
    {
        public string Bar;
    }

    [Immutable, Serializable]
    public class BarPublished : Event
    {
        public string Bar;
    }

    [Immutable, Serializable]
    public class Attach : Command
    {
        public ActorPath Sender;

        public Attach(ActorPath sender)
        {
            Sender = sender;
        }
    }

    [Immutable, Serializable]
    public class Detach : Command
    {
        public ActorPath Sender;

        public Detach(ActorPath sender)
        {
            Sender = sender;
        }
    }

    public interface ITestActor : IActor {}
}