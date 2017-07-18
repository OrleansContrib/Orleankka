using System;
using System.Collections.Generic;

using Orleankka;
using Orleankka.Embedded;
using Orleankka.Playground;
using Orleankka.Utility;

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

            EmbeddedActorSystem system;
            using (Trace.Execution("Full system startup"))
            {
                system = ActorSystem.Configure()
                    .Playground()
                    .Bootstrapper<ServiceLocator.Bootstrap>(properties)
                    .Assemblies(typeof(Api).Assembly)
                    .Done();

                system.Start().Wait();
                
            }
            client = new Client(system, system.CreateObservable().Result);
            client.Run();

            Console.WriteLine("Press Enter to terminate ...");
            Console.ReadLine();

            system.Dispose();
        }
    }
}
