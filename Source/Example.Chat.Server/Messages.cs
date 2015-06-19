using System;
using System.Linq;

namespace Example
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
}