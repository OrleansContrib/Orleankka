using System;
using System.Linq;

using Orleankka.Meta;

namespace Example
{
    public abstract class ChatUserCommand : Command 
    {}

    [Serializable]
    public class Join : ChatUserCommand
    {
        public string Room;
    }

    [Serializable]
    public class Leave : ChatUserCommand
    {
        public string Room;
    }

    [Serializable]
    public class Say : ChatUserCommand
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