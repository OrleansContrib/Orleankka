using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orleankka;

namespace Example.Chat.Server
{
    public class ChatServer : Actor
    {
        public ChatServer()
        {
            Users = new Dictionary<string, IObserverCollection>();
        }

        public Dictionary<string, IObserverCollection> Users { get; set; }

        private void NotifyClients(object message, Dictionary<string, IObserverCollection>.ValueCollection clients)
        {
            foreach (var user in clients)
            {
                user.Notify(message);
            }
        }

        public Task<object> Handle(JoinMessage message)
        {
            IObserverCollection clients;
            if (Users.TryGetValue(message.Username, out clients))
            {
                clients.Add(message.Client);
            }
            else
            {
                IObserverCollection newClientsCollection = new ObserverCollection();
                newClientsCollection.Add(message.Client);
                Users.Add(message.Username, newClientsCollection);
                message.Client.Notify(new NotificationMessage
                {
                    Text = "Hello and welcome to Orleankka chat example"
                });
            }


            NotifyClients(new NotificationMessage
            {
                Text = string.Format("User: {0} connected...", message.Username)
            }, Users.Values);


            return Task.FromResult(new object());
        }

        public Task<object> Handle(SayMessage message)
        {
            NotifyClients(new NewMessage
            {
                Username = message.Username,
                Text = message.Text
            }, Users.Values);

            return Task.FromResult(new object());
        }

        public Task<object> Handle(DisconnectMessage message)
        {
            IObserverCollection clients;
            if (Users.TryGetValue(message.Username, out clients))
            {
                if (clients.Count() == 1)
                {
                    Users.Remove(message.Username);
                }
                else
                {
                    clients.Remove(message.Client);
                }
            }
            return Task.FromResult(new object());
        }
    }
}