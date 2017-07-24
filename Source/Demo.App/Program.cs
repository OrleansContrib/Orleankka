using System;

using Orleankka;
using Orleankka.Embedded;
using Orleankka.Playground;
using Orleankka.Utility;

using Microsoft.WindowsAzure.Storage;

namespace Demo
{
    public static class Program
    {
        static Client client;

        public static void Main()
        {
            Console.WriteLine("Running demo. Booting cluster might take some time ...\n");

            var options = new Options {
                Account = CloudStorageAccount.DevelopmentStorageAccount
            };

            var activator = new DI();
            activator.Init(options).Wait();

            EmbeddedActorSystem system;
            using (Trace.Execution("Full system startup"))
            {
                system = ActorSystem.Configure()
                    .Playground()
                    .Activator(activator)
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
