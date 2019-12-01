using System;

using Orleankka;

using Orleans;

namespace Example
{
    public interface IChatUser : IActorGrain, IGrainWithStringKey
    {}

    [Serializable]
    public class Join : ActorMessage<IChatUser>
    {
        public string Room;
    }

    [Serializable]
    public class Leave : ActorMessage<IChatUser>
    {
        public string Room;
    }

    [Serializable]
    public class Say : ActorMessage<IChatUser>
    {
        public string Room;
        public string Message;
    }

    [Serializable]
    public class ChatRoomMessage
    {
        public string User;
        public string Text;
    }
}