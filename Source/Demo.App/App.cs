using System;
using System.Collections.Generic;
using System.Linq;

using Orleankka;

namespace Demo
{
    public class App
    {
        readonly IActorSystem system;
        readonly IClientObservable observable;

        public App(IActorSystem system, IClientObservable observable)
        {
            this.system = system;
            this.observable = observable;
        }

        public async void Run()
        {
            var facebook = system.ActorOf<IApi>("facebook");
            var twitter  = system.ActorOf<IApi>("twitter");

            await facebook.Tell(new Subscribe(observable.Ref));
            await twitter.Tell(new Subscribe(observable.Ref));

            observable.Subscribe(LogToConsole);

            foreach (var i in Enumerable.Range(1, 25))
            {
                var topic = system.ActorOf<ITopic>(i.ToString());

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
