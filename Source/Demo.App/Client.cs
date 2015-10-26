﻿using System;
using System.Collections.Generic;
using System.Linq;

using Orleankka;

namespace Demo
{
    public class Client
    {
        readonly IActorSystem system;
        readonly ClientObserver observer;

        public Client(IActorSystem system, ClientObserver observer)
        {
            this.system = system;
            this.observer = observer;
        }

        public async void Run()
        {
            var facebook = system.ActorOf<Api>("facebook");
            var twitter  = system.ActorOf<Api>("twitter");

            await facebook.Tell(new Subscribe(observer));
            await twitter.Tell(new Subscribe(observer));

            observer.Subscribe(LogToConsole);

            foreach (var i in Enumerable.Range(1, 25))
            {
                var topic = system.ActorOf<Topic>(i.ToString());

                await topic.Tell(new CreateTopic("[" + i + "]", new Dictionary<ActorRef, TimeSpan>
                {
                    {facebook, TimeSpan.FromMinutes(1)},
                    {twitter, TimeSpan.FromMinutes(1)},
                }));
            }
        }
    
        static void LogToConsole(object message)
        {
            var e = (AvailabilityChanged) message;

            Log.Message(
                !e.Available ? ConsoleColor.Red : ConsoleColor.Green,
                !e.Available ? "*{0}* gone wild. Unavailable!" : "*{0}* is back available again!", 
                e.Api);
        }
    }
}
