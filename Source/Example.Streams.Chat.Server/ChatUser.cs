using System;
using System.Linq;
using System.Threading.Tasks;

using Orleankka;
using Orleankka.Typed;

using Orleans.Providers.Streams.SimpleMessageStream;

namespace Example
{
    public class ChatUser : TypedActor
    {
        public async void Join(string room)
        {
            await Send(room, string.Format("{0} joined the room {1} ...", Id, room));
        }

        public async Task Leave(string room)
        {
            await Send(room, string.Format("{0} left the room {1}!", Id, room));
        }

        public Task Say(string room, string message)
        {
            return Send(room, string.Format("{0} said: {1}", Id, message));
        }

        Task Send(string room, string message)
        {
            Console.WriteLine("[server]: " + message);

            return GetStream(room).OnNextAsync(new ChatRoomMessage
            {
                User = Id,
                Text = message
            });
        }

        StreamRef GetStream(string room)
        {
            return System.StreamOf<SimpleMessageStreamProvider>(room);
        }
    }
}