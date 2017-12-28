using System;
using System.Reflection;

using Orleankka;
using Orleankka.Playground;

using static System.Console;

namespace Demo
{
    [Serializable]
    public class Greet
    {
        public string Who { get; set; }
    }

    public interface IGreeter : IActorGrain {}

    public class Greeter : ActorGrain, IGreeter
    {
        void On(Greet msg) => WriteLine($"Hello, {msg.Who}!");
    }

    public static class Program
    {   
        public static void Main()
        {
            WriteLine("Running example. Booting cluster might take some time ...\n");

            var system = ActorSystem.Configure()
                .Playground()
                .Assemblies(Assembly.GetExecutingAssembly())
                .Done();

            system.Start().Wait();

            var greeter = system.ActorOf<Greeter>("id");
            greeter.Tell(new Greet {Who = "world"}).Wait();

            Write("\n\nPress any key to terminate ...");
            ReadKey(true);
        }
    }
}
