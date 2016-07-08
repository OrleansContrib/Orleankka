using System;
using System.Threading.Tasks;

using Orleankka;

namespace Example
{
    class ChatClient
    {
        readonly ActorRef user;
        readonly StreamRef room;

        StreamSubscription subscription;

        public ChatClient(IActorSystem system, string user, string room)
        {
            this.user = system.ActorOf("ChatUser", user);
            this.room = system.StreamOf("sms", room);
        }

        public async Task Join()
        {
            subscription = await room.Subscribe<ChatRoomMessage>(message =>
            {
                if (message.User != UserName)
                    Console.WriteLine(message.Text);
            });

            await user.Tell(new Join {Room = RoomName});
        }

        public async Task Leave()
        {
            await subscription.Unsubscribe();
            await user.Tell(new Leave {Room = RoomName});
        }

        public async Task Say(string message)
        {
            await user.Tell(new Say {Room = RoomName, Message = message});
        }

        string UserName => user.Path.Id;
        string RoomName => room.Path.Id;
    }
}
