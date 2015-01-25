using System;
using System.Linq;

using Orleans.Concurrency;

namespace Orleankka
{
    [Immutable, Serializable]
    public class DoTell : Command
    {
        public readonly ActorPath Path;
        public readonly object Message;

        public DoTell(ActorPath path, object message)
        {
            Path = path;
            Message = message;
        }
    }

    [Immutable, Serializable]
    public class DoAsk : Query<object>
    {
        public readonly ActorPath Path;
        public readonly object Message;

        public DoAsk(ActorPath path, object message)
        {
            Path = path;
            Message = message;
        }
    }

    [Immutable, Serializable]
    public class DoAttach : Command
    {
        public readonly ActorPath Path;

        public DoAttach(ActorPath path)
        {
            Path = path;
        }
    }

    [Immutable, Serializable]
    public class GetReceivedNotifications : Query<Notification[]>
    {}

    public interface ITestInsideActor : IActor {}
}