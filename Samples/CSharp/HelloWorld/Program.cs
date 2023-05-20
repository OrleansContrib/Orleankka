using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;

using Orleans;
using Orleans.Hosting;

using Orleankka;
using Orleankka.Cluster;

using static System.Console;

namespace Demo
{
    [Serializable, GenerateSerializer]
    public class Greet
    {
        [Id(0)] public string Who { get; set; }
    }

    public interface IGreeter : IActorGrain, IGrainWithStringKey {}

    public class Greeter : DispatchActorGrain, IGreeter
    {
        void On(Greet msg) => WriteLine($"Hello, {msg.Who}!");
    }

    public static class Program
    {
        public static async Task Main()
        {
            WriteLine("Running example. Booting cluster might take some time ...\n");

            var host = new HostBuilder()
                .UseOrleans(c => c.UseLocalhostClustering())
                .UseOrleankka()
                .Build();

            await host.StartAsync();

            var greeter = host.ActorSystem().ActorOf<IGreeter>("id");
            await greeter.Tell(new Greet {Who = "world"});

            Write("\n\nPress any key to terminate ...");
            ReadKey(true);
        }
    }
}
