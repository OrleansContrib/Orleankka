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

            var stub = new ActorRefStub(path);
            unexpected.Add(path, stub);

            return stub;
        }

        public IEnumerable<ActorRefStub> Unexpected
        {
            get { return unexpected.Values; }
        }

        public void Verify()
        {
            // TODO: 
            //- Check unexpected ActorRef requests (no Mock was previously set), if there any - throw
            //- For all expected ActorRef requests, check unexpected Command and Queries - and throw
        }
    }
}
