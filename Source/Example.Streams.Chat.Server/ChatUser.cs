using System;
using System.Threading.Tasks;

using Orleankka;

namespace Example
{
    [ActorType("ChatUser")]
    public class ChatUser : Actor, IChatUser
    {
        Task On(Join x)   => Send(x.Room, $"{Id} joined the room {x.Room} ...");
        Task On(Leave x)  => Send(x.Room, $"{Id} left the room {x.Room}!");
        Task On(Say x)    => Send(x.Room, $"{Id} said: {x.Message}");

        Task Send(string room, string message)
        {
            Console.WriteLine("[server]: " + message);

            var stream = System.StreamOf("sms", room);

            return stream.Push(new ChatRoomMessage
            {
                User = Id,
                Text = message
            });
        }
    }
}