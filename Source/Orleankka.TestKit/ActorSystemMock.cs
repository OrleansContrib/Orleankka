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

        readonly Dictionary<ActorPath, ActorObserverStub> observers =
             new Dictionary<ActorPath, ActorObserverStub>();

        public ActorRefMock MockActorOf<TActor>(string id)
        {
            var path = new ActorPath(typeof(TActor), id);

            if (expected.ContainsKey(path))
                return expected[path];

            var mock = new ActorRefMock();
            expected.Add(path, mock);

            return mock;
        }

        ActorRef IActorSystem.ActorOf(ActorPath path)
        {
            if (expected.ContainsKey(path))
                return expected[path];

            if (unexpected.ContainsKey(path))
                return unexpected[path];

            var stub = new ActorRefStub();
            unexpected.Add(path, stub);

            return stub;
        }

        public IActorObserver ObserverOf(ActorPath path)
        {
            if (observers.ContainsKey(path))
                return observers[path];

            var stub = new ActorObserverStub();
            observers.Add(path, stub);

            return stub;
        }
    }
}
