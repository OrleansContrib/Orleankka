using System;
using System.Collections.Generic;

using Orleankka;

namespace Demo
{
    public static class Program
    {
        static Client client;

        public static void Main()
        {
            var properties = new Dictionary<string, string>
            {
                {"account", "UseDevelopmentStorage=true"}
            };

            var system = ActorSystem.Configure()
                .Playground()
                .Use<Bootstrap>(properties)
                .Register(typeof(Api).Assembly)
                .Done();

            client = new Client(system, Observer.Create().Result);
            client.Run();

            Console.WriteLine("Press Enter to terminate ...");
            Console.ReadLine();

            system.Dispose();
        }
    }
}
