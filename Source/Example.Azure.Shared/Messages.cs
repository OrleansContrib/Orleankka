using System;

using Orleankka;
using Orleankka.Meta;

namespace Example.Azure
{
    [Serializable]
    public class InitPublisher : Command
    {}

    [Serializable]
    public class SubscribeHub : Command
    {
        public ObserverRef Observer;
    }
}