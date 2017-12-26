using System;

using Orleankka;
using Orleankka.Embedded;
using Orleankka.Playground;
using Orleankka.Utility;

using Microsoft.WindowsAzure.Storage;
using Microsoft.Extensions.DependencyInjection;

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

            var storage = TopicStorage.Init(options.Account).GetAwaiter().GetResult();

            EmbeddedActorSystem system;
            using (Trace.Execution("Full system startup"))
            {
                system = ActorSystem.Configure()
                    .Playground()
                    .Cluster(c => c
                        .Services(s => s
                            .AddSingleton(storage)))
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
