using System;
using System.Collections.Generic;

namespace Orleankka.TestKit
{
    using Core;

    public class ActorSystemMock : IActorSystem
    {
        readonly Dictionary<ActorPath, ActorRefMock> expected =
             new Dictionary<ActorPath, ActorRefMock>();

        readonly Dictionary<ActorPath, ActorRefMock> unexpected =
             new Dictionary<ActorPath, ActorRefMock>();

        readonly IMessageSerializer serializer;

        public ActorSystemMock(IMessageSerializer serializer = null)
        {
            this.serializer = serializer ?? new BinarySerializer();
        }

        public ActorRefMock MockActorOf<TActor>(string id)
        {
            var path = ActorPath.From(typeof(TActor), id);

            if (expected.ContainsKey(path))
                return expected[path];

            var mock = new ActorRefMock(path, serializer);
            expected.Add(path, mock);

            return mock;
        }

        ActorRef IActorSystem.ActorOf(Type type, string id)
        {
            var path = ActorPath.From(type, id);
            return (this as IActorSystem).ActorOf(path);
        }

        ActorRef IActorSystem.ActorOf(ActorPath path)
        {
            if (expected.ContainsKey(path))
                return expected[path];

            if (unexpected.ContainsKey(path))
                return unexpected[path];

            var mock = new ActorRefMock(path);
            unexpected.Add(path, mock);

            return mock;
        }

        StreamRef IActorSystem.StreamOf(StreamPath path)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {}
    }
}
