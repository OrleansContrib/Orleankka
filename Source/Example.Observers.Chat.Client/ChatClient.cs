using System;
using System.Linq;
using System.Threading.Tasks;

using Orleankka;

namespace Example
{
    class ChatClient
    {
        readonly string user;
        readonly ActorRef room;
        Observer client;

        public ChatClient(IActorSystem system, string user, string room)
        {
            this.user = user;
            this.room = system.ActorOf<IChatRoom>(room);
        }

        public async Task Join()
        {
            client = await Observer.Create();
            client.Subscribe((ChatRoomMessage msg) =>
            {
                if (msg.User != user)
                    Console.WriteLine(msg.Text);
            });

            await room.Tell(new Join {User = user, Client = client});
        }

        public async Task Leave()
        {
            client.Dispose();
            await room.Tell(new Leave {User = user});
        }

        public async Task Say(string message)
        {
            await room.Tell(new Say {User = user, Message = message});
        }
    }
}
