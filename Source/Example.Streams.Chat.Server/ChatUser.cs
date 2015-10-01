using System;
using System.Linq;
using System.Threading.Tasks;

using Orleans;
using Orleans.Providers.Streams.SimpleMessageStream;

using Orleankka;

namespace Example
{
    public class ChatUser : Actor
    {
        Task On(Join x)   => Send(x.Room, $"{Id} joined the room {x.Room} ...");
        Task On(Leave x)  => Send(x.Room, $"{Id} left the room {x.Room}!");
        Task On(Say x)    => Send(x.Room, $"{Id} said: {x.Message}");

        Task Send(string room, string message)
        {
            Console.WriteLine("[server]: " + message);

            return GetStream(room).OnNextAsync(new ChatRoomMessage
            {
                User = Id,
                Text = message
            });
        }

        StreamRef GetStream(string room) => System.StreamOf<SimpleMessageStreamProvider>(room);
    }
}