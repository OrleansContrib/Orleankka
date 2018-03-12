using System;
using System.Reflection;
using System.Threading.Tasks;

using Orleankka;
using Orleankka.Client;
using Orleankka.Cluster;

using Orleans;
using Orleans.Hosting;

using static System.Console;

namespace Demo
{
    [Serializable]
    public class Greet
    {
        public string Who { get; set; }
    }

    public interface IGreeter : IActorGrain {}

    public class Greeter : DispatchActorGrain, IGreeter
    {
        void On(Greet msg) => WriteLine($"Hello, {msg.Who}!");
    }

    public static class Program
    {   
        public static async Task Main()
        {
            WriteLine("Running example. Booting cluster might take some time ...\n");

            var host = await new SiloHostBuilder()
                .ConfigureApplicationParts(x => x
                    .AddApplicationPart(Assembly.GetExecutingAssembly())
                    .WithCodeGeneration())
                .ConfigureOrleankka()
                .Start();

            var client = await host.Connect();
            var system = client.ActorSystem();
            
            var greeter = system.ActorOf<IGreeter>("id");
            await greeter.Tell(new Greet {Who = "world"});

            Write("\n\nPress any key to terminate ...");
            ReadKey(true);
        }
    }
}
