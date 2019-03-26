using System;
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
        public static IClientActorSystem ActorSystem;
        
        public static async Task<int> Main(string[] args)
        {
            ActorSystem = await RunOrleans();
            RunWeb(args);

            return 0;
        }

        static async Task<IClientActorSystem> RunOrleans()
        {
            var folder = CopierStorage.Init();

            silo = await new SiloHostBuilder()
                .ConfigureServices(s => s
                    .AddSingletonNamedService<IGrainStorage>("copier", (sp, __) =>
                    {
                        var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("CopierStorage");
                        return new CopierStorage(logger, folder);
                    }))
                .ConfigureApplicationParts(x => x
                    .AddApplicationPart(Assembly.GetExecutingAssembly())
                    .WithCodeGeneration())
                .UseOrleankka()
                .Start();

            var client = await silo.Connect();
            return client.ActorSystem();
        }

        static void RunWeb(string[] args)
        {
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>())
                .Build()
                .Run();
        }
    }
}
