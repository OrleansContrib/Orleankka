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
    public abstract class ActorObserverRequest : Command
    {
        public readonly IActorObserver Observer;

        protected ActorObserverRequest(IActorObserver observer)
        {
            Observer = observer;
        }
    }

    [Immutable, Serializable]
    public class Attach : ActorObserverRequest
    {
        public Attach(IActorObserverProxy observer)
            : base(observer.Proxy)
        {}
    }

    [Immutable, Serializable]
    public class Detach : ActorObserverRequest
    {
        public Detach(IActorObserverProxy observer)
            : base(observer.Proxy)
        {}
    }

    public interface ITestActor : IActor {}
}