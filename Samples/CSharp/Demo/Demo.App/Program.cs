using System;
using System.Threading.Tasks;

using Microsoft.WindowsAzure.Storage;
using Microsoft.Extensions.DependencyInjection;

using Orleankka;
using Orleankka.Client;
using Orleankka.Cluster;

using Orleans;
using Orleans.Hosting;

namespace Demo
{
    public static class Program
    {
        static App app;

        public static async Task Main()
        {
            Console.WriteLine("Make sure you've started Azure storage emulator!");
            Console.WriteLine("Running demo. Booting cluster might take some time ...\n");

            var account = CloudStorageAccount.DevelopmentStorageAccount;
            var storage = await TopicStorage.Init(account);

            var host = await new SiloHostBuilder()
                .ConfigureServices(x => x
                    .AddSingleton(storage))
                .ConfigureApplicationParts(x => x
                    .AddApplicationPart(typeof(Api).Assembly)
                    .WithCodeGeneration())
                .UseOrleankka()
                .Start();

            var client = await host.Connect();
            var system = client.ActorSystem();

            app = new App(system, await system.CreateObservable());
            app.Run();

            Console.WriteLine("Press Enter to terminate ...");
            Console.ReadLine();

            client.Dispose();
            host.Dispose();
        }
    }
}
