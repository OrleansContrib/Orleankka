﻿using System;
using System.Threading.Tasks;

using Orleankka;

namespace Example
{
    class ChatClient
    {
        readonly string user;
        readonly ActorRef room;
        ClientObservable notifications;

        public ChatClient(IActorSystem system, string user, string room)
        {
            this.user = user;
            this.room = system.ActorOf($"ChatRoom:{room}");
        }

        public async Task Join()
        {
            notifications = await ClientObservable.Create();
            notifications.Subscribe((ChatRoomMessage msg) =>
            {
                if (msg.User != user)
                    Console.WriteLine(msg.Text);
            });

            await room.Tell(new Join {User = user, Client = notifications});
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
