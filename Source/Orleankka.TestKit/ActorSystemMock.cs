using System;
using System.Collections.Generic;
using System.Linq;

namespace Orleankka.TestKit
{
    public class ActorSystemMock : IActorSystem
    {
        readonly Dictionary<ActorPath, ActorRefMock> expected =
            new Dictionary<ActorPath, ActorRefMock>();

        readonly Dictionary<ActorPath, ActorRefStub> unexpected =
            new Dictionary<ActorPath, ActorRefStub>();

        readonly Dictionary<ActorObserverPath, IActorObserver> observers =
             new Dictionary<ActorObserverPath, IActorObserver>();

        public ActorRefMock MockActorOf<TActor>(string id)
        {
            var path = new ActorPath(typeof(TActor), id);

            if (expected.ContainsKey(path))
                return expected[path];

            var mock = new ActorRefMock();
            expected.Add(path, mock);

            return mock;
        }

        IActorRef IActorSystem.ActorOf(ActorPath path)
        {
            if (expected.ContainsKey(path))
                return expected[path];

            if (unexpected.ContainsKey(path))
                return unexpected[path];

            var stub = new ActorRefStub();
            unexpected.Add(path, stub);

            return stub;
        }

        public IActorObserver ObserverOf(ActorObserverPath path)
        {
            if (observers.ContainsKey(path))
                return observers[path];

            var stub = new ActorObserverStub();
            observers.Add(path, stub);

            return stub;
        }

        public ActorObserverPath PathOf(IActorObserver observer)
        {
            if (observers.ContainsValue(observer))
                return observers.Single(x => x.Value == observer).Key;

            var path = new ActorObserverPath(Guid.NewGuid().ToString("D"));
            observers.Add(path, new ActorObserverStub());

            return path;
        }
    }
}
