using System;
using System.Threading.Tasks;

using Orleans;
using Orleans.Streams;

using Orleankka;

namespace Example
{
    class ChatClient
    {
        readonly ActorRef user;
        readonly StreamRef room;

        StreamSubscriptionHandle<object> subscription;

        public ChatClient(IActorSystem system, string user, string room)
        {
            this.user = system.ActorOf<ChatUser>(user);
            this.room = system.StreamOf("sms", room);
        }

        public async Task Join()
        {
            subscription = await room.SubscribeAsync<ChatRoomMessage>((msg, token) =>
            {
                if (msg.User != UserName)
                    Console.WriteLine(msg.Text);

                return TaskDone.Done;
            });

            await user.Tell(new Join {Room = RoomName});
        }

        public async Task Leave()
        {
            await subscription.UnsubscribeAsync();
            await user.Tell(new Leave {Room = RoomName});
        }

        public async Task Say(string message)
        {
            await user.Tell(new Say {Room = RoomName, Message = message});
        }

        string UserName
        {
            get { return user.Path.Id; }
        }        
        
        string RoomName
        {
            get { return room.Path.Id; }
        }
    }
}
