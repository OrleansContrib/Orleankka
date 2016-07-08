using System;
using System.Threading.Tasks;

using Orleankka;
using Orleankka.CSharp;

namespace Example
{
    [ActorTypeCode("ChatUser")]
    public class ChatUser : Actor
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