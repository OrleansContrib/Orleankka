using System;
using Orleankka;

namespace Server
{
    [Serializable]
    public class NotificationMessage
    {
        public string Text { get; set; }
    }

    [Serializable]
    public class NewMessage
    {
        public string Username { get; set; }
        public string Text { get; set; }
    }

    [Serializable]
    public class SayMessage
    {
        public string Username { get; set; }
        public string Text { get; set; }
    }

    [Serializable]
    public class DisconnectMessage
    {
        public string Username { get; set; }
        public ObserverRef Client { get; set; }
    }

    [Serializable]
    public class JoinMessage
    {
        public string Username { get; set; }
        public ObserverRef Client { get; set; }
    }
}