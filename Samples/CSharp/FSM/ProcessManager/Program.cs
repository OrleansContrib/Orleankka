using System;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Orleans;
using Orleankka;
using Orleankka.Client;
using Orleankka.Cluster;

using Orleans.Hosting;
using Orleans.Runtime;
using Orleans.Storage;

using static System.Console;

namespace Example
{
    public static class Program
    {   
        public static async Task Main()
        {
            WriteLine("Running example. Booting cluster might take some time ...\n");

            var folder = CopierStorage.Init();

            var host = await new SiloHostBuilder()
                .ConfigureServices(s =>
                {
                    s.AddSingletonNamedService<IGrainStorage>("copier", (sp, __) =>
                    {
                        var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("CopierStorage");
                        return new CopierStorage(logger, folder);
                    });
                })
                .ConfigureApplicationParts(x => x
                    .AddApplicationPart(Assembly.GetExecutingAssembly())
                    .WithCodeGeneration())
                .UseOrleankka()
                .Start();

            var client = await host.Connect();
            await Run(client.ActorSystem());

            Write("\n\nPress any key to terminate ...");
            ReadKey(true);

            host.Dispose();
            Environment.Exit(0);
        }

        static async Task Run(IActorSystem system)
        {
            var copier = system.TypedActorOf<ICopier>("1");
            await copier.Tell(new Start());
        }
    }
}
