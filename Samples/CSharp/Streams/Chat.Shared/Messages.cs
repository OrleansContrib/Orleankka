using System;

using Orleankka;

using Orleans;

namespace Example
{
    public interface IChatUser : IActorGrain, IGrainWithStringKey
    {}

    [Serializable, GenerateSerializer]
    public class Join : ActorMessage<IChatUser>
    {
        [Id(0)]
        public string Room;
    }

    [Serializable, GenerateSerializer]
    public class Leave : ActorMessage<IChatUser>
    {
        [Id(0)]
        public string Room;
    }

    [Serializable, GenerateSerializer]
    public class Say : ActorMessage<IChatUser>
    {
        [Id(0)]
        public string Room;
        [Id(1)]
        public string Message;
    }

    [Serializable, GenerateSerializer]
    public class ChatRoomMessage
    {
        [Id(0)]
        public string User;
        [Id(1)]
        public string Text;
    }
}