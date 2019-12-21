using System;
using System.Threading.Tasks;

using Orleankka;

namespace Example
{
    public class ChatUser : DispatchActorGrain, IChatUser
    {
        Task On(Join x)   => Send(x.Room, $"{Id} joined the room {x.Room} ...");
        Task On(Leave x)  => Send(x.Room, $"{Id} left the room {x.Room}!");
        Task On(Say x)    => Send(x.Room, $"{Id} said: {x.Message}");

        Task Send(string room, string message)
        {
            Console.WriteLine("[server]: " + message);

            var stream = System.StreamOf("sms", room);

            return stream.Publish(new ChatRoomMessage
            {
                User = Id,
                Text = message
            });
        }
    }
}