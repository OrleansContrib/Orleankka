using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Orleankka.Core;

namespace Orleankka.TestKit
{
    public class ActorSystemMock : IActorSystem
    {
        readonly Dictionary<ActorPath, ActorRefMock> expected =
            new Dictionary<ActorPath, ActorRefMock>();

        readonly Dictionary<ActorPath, ActorRefStub> unexpected =
            new Dictionary<ActorPath, ActorRefStub>();

        public ActorSystemMock()
        {
            ActorAssembly.Register(new[] {Assembly.GetCallingAssembly()});
        }

        public ActorSystemMock(params Assembly[] assemblies)
        {
            ActorAssembly.Register(assemblies);
        }

        #region IActorSystem Members

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

        StreamRef IActorSystem.StreamOf(StreamPath path)
        {
            return new StreamRefStub(path);
        }

        public void Dispose()
        {
            ActorAssembly.Reset();
        }

        #endregion

        public ActorRefMock MockActorOf<TActor>(string id)
        {
            var path = ActorPath.From(typeof(TActor), id);

            if (expected.ContainsKey(path))
                return expected[path];

            var mock = new ActorRefMock(path);
            expected.Add(path, mock);

            return mock;
        }
    }
}