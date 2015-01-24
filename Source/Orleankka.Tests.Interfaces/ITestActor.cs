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
        public ActorObserverPath Path;

        public Attach(ActorObserverPath path)
        {
            Path = path;
        }
    }

    [Immutable, Serializable]
    public class Detach : Command
    {
        public ActorObserverPath Path;

        public Detach(ActorObserverPath path)
        {
            Path = path;
        }
    }

    public interface ITestActor : IActor {}
}