using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orleankka.TestKit
{
    using Client;

    public class ActorSystemMock : IClientActorSystem
    {
        readonly MessageSerialization serialization;

        readonly Dictionary<ActorPath, ActorRefMock> actors =
             new Dictionary<ActorPath, ActorRefMock>();

        readonly Queue<ClientObservableMock> observables = 
             new Queue<ClientObservableMock>();

        public ActorSystemMock(MessageSerialization serialization = null)
        {
            this.serialization = serialization ?? MessageSerialization.Default;
        }

        public ActorRefMock MockActorOf<TActor>(string id)
        {
            var path = ActorPath.For(typeof(TActor), id);
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

            var mock = new ActorRefMock(path, serialization);
            actors.Add(path, mock);

            return mock;
        }

        public ClientObservableMock MockCreateObservable()
        {
            var mock = new ClientObservableMock();
            observables.Enqueue(mock);
            return mock;
        }

        IClientObservable IClientActorSystem.CreateObservable()
        {
            if (observables.Count == 0)
                throw new InvalidOperationException(
                    "No mock has been previosly setup for this client observable request.\n" +
                    $"Use {nameof(MockCreateObservable)} method to setup.");
            
            return observables.Dequeue();
        }

        ClientRef IActorSystem.ClientOf(string path)
        {
            throw new NotImplementedException();
        }
        
        public void Reset()
        {
            actors.Clear();
        }

        public void Dispose()
        {}
    }
}
