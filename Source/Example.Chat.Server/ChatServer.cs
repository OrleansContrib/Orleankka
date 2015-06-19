using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Orleans;
using Orleankka;

namespace Example
{
    public class ChatServer : Orleankka.Typed.TypedActor
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

        public Task Join(string username, ObserverRef client)
        {
            IObserverCollection clients;
            if (Users.TryGetValue(username, out clients))
            {
                clients.Add(client);
            }
            else
            {
                IObserverCollection newClientsCollection = new ObserverCollection();
                newClientsCollection.Add(client);
                Users.Add(username, newClientsCollection);
                client.Notify(new NotificationMessage
                {
                    Text = "Hello and welcome to Orleankka chat example"
                });
            }


            NotifyClients(new NotificationMessage
            {
                Text = string.Format("User: {0} connected...", username)
            }, Users.Values);

            return TaskDone.Done;
        }

        public Task Say(string username, string text)
        {
            NotifyClients(new NewMessage
            {
                Username = username,
                Text = text
            }, Users.Values);
            return TaskDone.Done;
        }

        public Task Disconnect(string username, ObserverRef client)
        {
            IObserverCollection clients;
            if (Users.TryGetValue(username, out clients))
            {
                if (clients.Count() == 1)
                {
                    Users.Remove(username);
                }
                else
                {
                    clients.Remove(client);
                }
            }
            return TaskDone.Done;
        }
    }
}