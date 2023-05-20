using System;

using Orleankka;

namespace Example
{
    using Orleans;

    [Serializable, GenerateSerializer]
    public class Join
    {
        [Id(0)] public string User;
        [Id(1)] public ObserverRef Client;
    }

    [Serializable, GenerateSerializer]
    public class Leave
    {
        [Id(0)] public string User;
    }

    [Serializable, GenerateSerializer]
    public class Say
    {
        [Id(0)] public string User;
        [Id(1)] public string Message;
    }

    [Serializable, GenerateSerializer]
    public class ChatRoomMessage
    {
        [Id(0)] public string User; 
        [Id(1)] public string Text;
    }
}