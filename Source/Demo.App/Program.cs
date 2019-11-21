using System;
using System.Threading.Tasks;

using Orleankka;
using Orleankka.Embedded;
using Orleankka.Playground;
using Orleankka.Utility;

using Microsoft.WindowsAzure.Storage;
using Microsoft.Extensions.DependencyInjection;

using Orleans;
using Orleans.Runtime;

namespace Demo
{
    public static class Program
    {
        static Client client;

        public static async Task Main()
        {
            Console.WriteLine("Running demo. Booting cluster might take some time ...\n");

            var storage = await TopicStorage.Init(CloudStorageAccount.DevelopmentStorageAccount);

            EmbeddedActorSystem system;
            using (Trace.Execution("Full system startup"))
            {
                system = ActorSystem.Configure()
                    .Playground()
                    .Cluster(c => c
                        .Builder(b => b.UseDashboard(_ => {}))
                        .Services(s => s
                            .AddSingleton<ITopicStorage>(storage)
                            .AddSingleton<IGrainActivator>(sp => new DI(sp))))
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
