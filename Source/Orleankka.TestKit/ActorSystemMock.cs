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

        readonly Dictionary<ObserverPath, ObserverRefStub> observers =
             new Dictionary<ObserverPath, ObserverRefStub>();

        public ActorRefMock MockActorOf<TActor>(string id)
        {
            var path = ActorPath.From(typeof(TActor), id);

            if (expected.ContainsKey(path))
                return expected[path];

            var mock = new ActorRefMock(path);
            expected.Add(path, mock);

            return mock;
        }

        ActorRef IActorSystem.ActorOf(ActorPath path)
        {
            if (expected.ContainsKey(path))
                return expected[path];

            if (unexpected.ContainsKey(path))
                return unexpected[path];

            var stub = new ActorRefStub(path);
            unexpected.Add(path, stub);

            return stub;
        }

        ObserverRef IActorSystem.ObserverOf(ObserverPath path)
        {
            if (observers.ContainsKey(path))
                return observers[path];

            var stub = new ObserverRefStub(path);
            observers.Add(path, stub);

            return stub;
        }
    }
}
