using System;
using System.Linq;

namespace Orleankka.TestKit
{
    public class ActorObserverStub : IActorObserver
    {
        public void OnNext(Notification notification)
        {}
    }
}