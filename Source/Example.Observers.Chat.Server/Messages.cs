using System;
using System.Linq;

using Orleankka;

namespace Example
{
    [Serializable]
    public class Join
    {
        public string User;
        public ObserverRef Client;
    }

    [Serializable]
    public class Leave
    {
        public string User;
    }

    [Serializable]
    public class Say
    {
        public string User;
        public string Message;
    }

    [Serializable]
    public class ChatRoomMessage
    {
        public string User;
        public string Text;
    }
}