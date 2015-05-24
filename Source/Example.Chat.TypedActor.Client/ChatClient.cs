using System;
using System.Threading.Tasks;
using Example.Chat.TypedActor.Server;
using Orleankka;
using Orleankka.Typed;

namespace Example.Chat.TypedActor.Client
{
    public class ChatClient
    {
        public ChatClient()
        {
        }

        public ChatClient(TypedActorRef<ChatServer> server, ObserverRef clientRef, string userName)
        {
            Server = server;
            ClientRef = clientRef;
            UserName = userName;
        }

        public TypedActorRef<ChatServer> Server { get; set; }
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
            await Server.Call(c => c.Join(UserName, ClientRef));
        }

        public async Task Disconnect()
        {
            await Server.Call(c => c.Disconnect(UserName, ClientRef));
        }

        public async Task Say(string text)
        {
            await Server.Call(c => c.Say(UserName, text));
        }
    }
}