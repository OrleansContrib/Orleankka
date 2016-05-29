using System;
using System.Collections.Generic;

namespace Orleankka.TestKit
{
    public class ActorSystemMock : IActorSystem
    {
        readonly Dictionary<ActorPath, ActorRefMock> actors =
             new Dictionary<ActorPath, ActorRefMock>();

        readonly Dictionary<StreamPath, StreamRefMock> streams =
             new Dictionary<StreamPath, StreamRefMock>();

        public ActorRefMock MockActorOf<TActor>(string id)
        {
            var path = typeof(TActor).ToActorPath(id);
            return GetOrCreateMock(path);
        }

        public ActorRefMock MockActorOf(ActorPath path)
        {
            return GetOrCreateMock(path);
        }

        ActorRef IActorSystem.ActorOf(ActorPath path)
        {
            return GetOrCreateMock(path);
        }

        ActorRefMock GetOrCreateMock(ActorPath path)
        {
            if (actors.ContainsKey(path))
                return actors[path];

            var mock = new ActorRefMock(path);
            actors.Add(path, mock);

            return mock;
        }

        public StreamRefMock MockStreamOf(string provider, string id)
        {
            var path = StreamPath.From(provider, id);
            return GetOrCreateMock(path);
        }

        StreamRef IActorSystem.StreamOf(StreamPath path)
        {
            return GetOrCreateMock(path);
        }

        StreamRefMock GetOrCreateMock(StreamPath path)
        {
            if (streams.ContainsKey(path))
                return streams[path];

            var mock = new StreamRefMock(path);
            streams.Add(path, mock);

            return mock;
        }

        public void Reset()
        {
            actors.Clear();
            streams.Clear();
        }

        public void Dispose()
        {}
    }
}
