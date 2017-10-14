using System;
using System.Threading.Tasks;

using Orleankka;
using Orleankka.Client;

namespace Example
{
    class ChatClient
    {
        readonly IClientActorSystem system;
        readonly string user;
        readonly ActorRef room;
        IClientObservable notifications;

        public ChatClient(IClientActorSystem system, string user, string room)
        {
            this.system = system;
            this.user = user;
            this.room = system.ActorOf($"ChatRoom:{room}");
        }

        public async Task Join()
        {
            notifications = await system.CreateObservable();
            notifications.Subscribe((ChatRoomMessage msg) =>
            {
                if (msg.User != user)
                    Console.WriteLine(msg.Text);
            });

            await room.Tell(new Join {User = user, Client = notifications.Ref});
        }

        public async Task Leave()
        {
            notifications.Dispose();
            await room.Tell(new Leave {User = user});
        }

        public async Task Say(string message)
        {
            await room.Tell(new Say {User = user, Message = message});
        }
    }
}
