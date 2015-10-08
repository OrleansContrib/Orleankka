using System;
using System.Collections.Generic;

namespace Orleankka.TestKit
{
    using Core;

    public class ActorSystemMock : IActorSystem
    {
        readonly Dictionary<ActorPath, ActorRefMock> actors =
             new Dictionary<ActorPath, ActorRefMock>();

        readonly Dictionary<StreamPath, StreamRefMock> streams =
             new Dictionary<StreamPath, StreamRefMock>();

        readonly IMessageSerializer serializer;

        public ActorSystemMock(IMessageSerializer serializer = null)
        {
            this.serializer = serializer ?? MessageEnvelope.Serializer;
        }

        public ActorRefMock MockActorOf<TActor>(string id)
        {
            var path = ActorPath.From(typeof(TActor), id);
            return GetOrCreateMock(path);
        }

        ActorRef IActorSystem.ActorOf(Type type, string id)
        {
            var path = ActorPath.From(type, id);
            return (this as IActorSystem).ActorOf(path);
        }

        ActorRef IActorSystem.ActorOf(ActorPath path)
        {
            return GetOrCreateMock(path);
        }

        ActorRefMock GetOrCreateMock(ActorPath path)
        {
            if (actors.ContainsKey(path))
                return actors[path];

            var mock = new ActorRefMock(path, serializer);
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

            var mock = new StreamRefMock(path, serializer);
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
