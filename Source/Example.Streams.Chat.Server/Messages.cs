using System;
using System.Linq;

namespace Example
{
    [Serializable]
    public class Join
    {
        public string Room;
    }

    [Serializable]
    public class Leave
    {
        public string Room;
    }

    [Serializable]
    public class Say
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