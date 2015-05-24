using System;
using System.Threading.Tasks;
using Example.Chat.Server;
using Orleankka;

namespace Example.Chat.Client
{
    public class ChatClient
    {
        public ChatClient()
        {
        }

        public ChatClient(ActorRef server, ObserverRef clientRef, string userName)
        {
            Server = server;
            ClientRef = clientRef;
            UserName = userName;
        }

        public ActorRef Server { get; set; }
        public ObserverRef ClientRef { get; set; }
        public string UserName { get; set; }

        public void Handle(NewMessage message)
        {
            Console.WriteLine("{0}: {1}", message.Username, message.Text);
        }

        public void Handle(NotificationMessage message)
        {
            Console.WriteLine("{0}", message.Text);
        }

        public async Task Join()
        {
            await Server.Tell(new JoinMessage
            {
                Client = ClientRef,
                Username = UserName
            });
        }

        public async Task Disconnect()
        {
            await Server.Tell(new DisconnectMessage
            {
                Client = ClientRef,
                Username = UserName
            });
        }

        public async Task Say(string text)
        {
            await Server.Tell(new SayMessage
            {
                Username = UserName,
                Text = text
            });
        }
    }
}