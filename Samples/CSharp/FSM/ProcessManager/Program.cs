using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Orleankka.Cluster;

using Orleans.Hosting;
using Orleans.Runtime;
using Orleans.Storage;

namespace ProcessManager
{
    using Orleankka;
    using Orleankka.Client;

    public static class Program
    {
        static IHost host;
        
        public static async Task<int> Main()
        {
            var system = await RunCluster();
            RunGui(system);

            return 0;
        }

        static async Task<IClientActorSystem> RunCluster()
        {
            var folder = Storage.Init();

            host = await SiloHostBuilderExtension.UseOrleankka(new HostBuilder()
                    .ConfigureServices(s => s
                        .AddKeyedSingleton<IGrainStorage>("copier",  (sp, __) => new Storage(sp, typeof(CopierState), folder)))
                    .UseOrleans(c => c
                        .AddMemoryStreams("notifications")))
                .StartServer();

            return SiloHostBuilderExtension.ActorSystem(host);
        }

        static void RunGui(IClientActorSystem system)
        {
            Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureServices(x => x.AddSingleton(system));
                    webBuilder.UseStartup<Startup>();
                })
                .Build()
                .Run();
        }
    }
}
