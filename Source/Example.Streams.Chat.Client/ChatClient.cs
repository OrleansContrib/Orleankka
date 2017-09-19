using System;
using System.Threading.Tasks;

using Orleankka;

namespace Example
{
    class ChatClient
    {
        readonly ActorRef<IChatUser> user;
        readonly IActorSystem system;
        StreamSubscription subscription;

        public ChatClient(IActorSystem system, string user, string room)
        {
            this.user = system.TypedActorOf<IChatUser>(user);
            this.system = system;
            RoomName = room;
        }

        public async Task Join()
        {
            await Subscribe();
            await user.Tell(new Join {Room = RoomName});
        }

        async Task Subscribe()
        {
            var room = system.StreamOf("sms", RoomName);
            subscription = await room.Subscribe<ChatRoomMessage>(message =>
            {
                if (message.User != UserName)
                    Console.WriteLine(message.Text);
            });
        }

        public Task Resubscribe() => Subscribe();

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
        string RoomName { get; }
    }
}
