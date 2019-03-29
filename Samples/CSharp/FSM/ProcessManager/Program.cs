using System.Reflection;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Orleankka;
using Orleankka.Client;
using Orleankka.Cluster;

using Orleans;
using Orleans.Hosting;
using Orleans.Runtime;
using Orleans.Storage;

namespace ProcessManager
{
    public static class Program
    {
        static ISiloHost silo;
        
        public static async Task<int> Main()
        {
            var system = await RunCluster();
            RunGui(system);

            return 0;
        }

        static async Task<IClientActorSystem> RunCluster()
        {
            var folder = Storage.Init();

            silo = await new SiloHostBuilder()
                .ConfigureServices(s => s
                    .AddSingletonNamedService<IGrainStorage>("copier",  (sp, __) => new Storage(sp, typeof(CopierState), folder))
                    .AddSingletonNamedService<IGrainStorage>("manager", (sp, __) => new Storage(sp, typeof(ManagerState), folder)))
                .AddSimpleMessageStreamProvider("notifications", o =>
                {
                    o.FireAndForgetDelivery = false;
                    o.OptimizeForImmutableData = false;
                })
                .ConfigureApplicationParts(x => x
                    .AddApplicationPart(Assembly.GetExecutingAssembly())
                    .WithCodeGeneration())
                .UseOrleankka()
                .Start();

            return silo.ActorSystem();
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
