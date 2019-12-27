using System;
using System.Threading.Tasks;

using Orleankka;

namespace Example
{
    class ChatClient
    {
        readonly ActorRef<IChatUser> user;
        readonly IActorSystem system;
        StreamSubscription<ChatRoomMessage> subscription;

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
            var room = system.StreamOf<ChatRoomMessage>("sms", RoomName);
            subscription = await room.Subscribe((message, _) =>
            {
                if (message.User != UserName)
                    Console.WriteLine(message.Text);
            });
        }

        public async Task Resubscribe()
        {
            while (true)
            {
                try
                {
                    await Subscribe();
                    break;
                }
                catch
                {
                    Console.WriteLine("Error resubscribing to stream. Will retry in 2 seconds");
                    await Task.Delay(2000);
                }
            }
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
        string RoomName { get; }
    }
}
