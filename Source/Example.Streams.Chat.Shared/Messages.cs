using System;

using Orleankka;
using Orleankka.CSharp;

namespace Example
{
    [ActorType("ChatUser")]
    public interface IChatUser : IActor
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