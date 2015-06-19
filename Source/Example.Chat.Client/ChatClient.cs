using System;
using System.Linq;
using System.Threading.Tasks;

using Orleankka;
using Orleankka.Typed;

namespace Example
{
    public class ChatClient : IDisposable
    {
        private IDisposable _observer;
        protected Observer Client;
        protected TypedActorRef<ChatServer> Server;

        public ChatClient()
        {
        }

        public ChatClient(TypedActorRef<ChatServer> server, string userName)
        {
            Server = server;
            UserName = userName;
        }

        public string UserName { get; set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async Task Start()
        {
            Client = await Observer.Create();

            _observer = Client.Subscribe(o => Handle((dynamic) o));
        }

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
            await Server.Call(c => c.Join(UserName, Client.Ref));
        }

        public async Task Disconnect()
        {
            await Server.Call(c => c.Disconnect(UserName, Client.Ref));
        }

        public async Task Say(string text)
        {
            await Server.Call(c => c.Say(UserName, text));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_observer != null) _observer.Dispose();
            }
        }
    }
}