using System;
using System.Collections.Generic;

using Orleankka;
using Orleankka.Playground;

namespace Demo
{
    public static class Program
    {
        static Client client;

        public static void Main()
        {
            Console.WriteLine("Running demo. Booting cluster might take some time ...\n");

            var properties = new Dictionary<string, string>
            {
                {"account", "UseDevelopmentStorage=true"}
            };

            var system = ActorSystem.Configure()
                .Playground()
                .Run<ServiceLocator.Bootstrap>(properties)
                .Register(typeof(Api).Assembly)
                .Done();

            system.Start();

            client = new Client(system, ClientObservable.Create().Result);
            client.Run();

            Console.WriteLine("Press Enter to terminate ...");
            Console.ReadLine();

            system.Dispose();
        }
    }
}
