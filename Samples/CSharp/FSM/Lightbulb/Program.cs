using System;
using System.Reflection;
using System.Threading.Tasks;

using Orleans;
using Orleankka;
using Orleankka.Client;
using Orleankka.Cluster;

using Orleans.Hosting;
using Orleans.Runtime.Configuration;

using static System.Console;

namespace Example
{
    public static class Program
    {   
        public static async Task Main()
        {
            WriteLine("Running example. Booting cluster might take some time ...\n");

            var host = await new SiloHostBuilder()
                .UseConfiguration(ClusterConfiguration.LocalhostPrimarySilo())
                .ConfigureApplicationParts(x => x
                    .AddApplicationPart(Assembly.GetExecutingAssembly())
                    .WithCodeGeneration())
                .ConfigureOrleankka()
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
            var lightbulb = system.ActorOf<ILightbulb>("eco");
            
            async Task Request(object message)
            {
                var response = await lightbulb.Ask<string>(message);
                WriteLine(response);
            }

            await Request(new PressSwitch());
            await Request(new Touch());
            await Request(new PressSwitch());
            await Request(new Touch());

            await Request(new HitWithHammer());
            await Request(new PressSwitch());
            await Request(new Touch());
        }
    }
}
