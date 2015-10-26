﻿using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Orleankka;
using Orleankka.Playground;

namespace Example.Serialization.JSON
{
    public static class Program
    {
        public static void Main()
        {
            var system = ActorSystem.Configure()
                .Playground()
                .Register(Assembly.GetExecutingAssembly())
                .Serializer<JsonSerializer>()
                .Done();

            Run(system).Wait();

            Console.WriteLine("Press Enter to terminate ...");
            Console.ReadLine();

            system.Dispose();
        }

        static async Task Run(IActorSystem system)
        {
            var e0 = system.TypedActorOf<Employee>("E0");
            var e1 = system.TypedActorOf<Employee>("E1");
            var e2 = system.TypedActorOf<Employee>("E2");
            var e3 = system.TypedActorOf<Employee>("E3");
            var e4 = system.TypedActorOf<Employee>("E4");

            var m0 = system.TypedActorOf<Manager>("M0");
            var m1 = system.TypedActorOf<Manager>("M1");

            await m0.Tell(new AddDirectReport {Employee = e0});
            await m0.Tell(new AddDirectReport {Employee = e1});
            await m0.Tell(new AddDirectReport {Employee = e2});

            await m1.Tell(new AddDirectReport {Employee = e3});
            await m1.Tell(new AddDirectReport {Employee = e4});

            await e1.Tell(new Promote {NewLevel = 80});
            await e4.Tell(new Promote {NewLevel = 80});
        }
    }
}
