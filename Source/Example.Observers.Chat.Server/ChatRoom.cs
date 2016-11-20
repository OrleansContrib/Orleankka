using System;
using System.Collections.Generic;

using Orleankka;

namespace Example
{
    [ActorType("ChatRoom")]
    public class ChatRoom : Actor
    {
        readonly HashSet<string> members =
             new HashSet<string>();

        readonly Dictionary<string, IObserverCollection> online = 
             new Dictionary<string, IObserverCollection>();

        public void Handle(Join cmd)
        {
            var firstTimeJoined = members.Add(cmd.User);
            if (firstTimeJoined)
                Send(cmd.User, string.Format("{1} joined the room {0} ...", Id, cmd.User));

            IObserverCollection connections;
            if (!online.TryGetValue(cmd.User, out connections))
            {
                connections = new ObserverCollection();
                online.Add(cmd.User, connections);
            }

            connections.Add(cmd.Client);
        }

        public void Handle(Leave cmd)
        {
            online.Remove(cmd.User);

            var hasJoinedBefore = members.Remove(cmd.User);
            if (hasJoinedBefore)
                Send(cmd.User, string.Format("{1} left the room {0}!", Id, cmd.User));
        }

        public void Handle(Say cmd)
        {
            Send(cmd.User, $"{cmd.User} said: {cmd.Message}");
        }

        void Send(string user, string message)
        {
            Console.WriteLine("[server]: " + message);

            foreach (var connection in online.Values)
            {
                connection.Notify(new ChatRoomMessage
                {
                    User = user,
                    Text = message
                });                
            }
        }
    }
}