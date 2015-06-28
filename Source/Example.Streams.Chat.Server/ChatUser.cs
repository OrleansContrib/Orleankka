using System;
using System.Linq;
using System.Threading.Tasks;

using Orleans.Providers.Streams.SimpleMessageStream;
using Orleankka;

namespace Example
{
    public partial class ChatUser : UntypedActor
    {
        protected override void Define()
        {
            On(async (Join x)   => await Send(x.Room, string.Format("{0} joined the room {1} ...", Id, x.Room)));
            On(async (Leave x)  => await Send(x.Room, string.Format("{0} left the room {1}!", Id, x.Room)));
            On(async (Say x)    => await Send(x.Room, string.Format("{0} said: {1}", Id, x.Message)));
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