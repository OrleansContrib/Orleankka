using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Orleans;
using Orleankka;
using Orleankka.Playground;

namespace Example
{
    public static class Program
    {   
        public static void Main()
        {
            Console.WriteLine("Running example. Booting cluster might take some time ...\n");

            var system = ActorSystem.Configure()
                .Playground()
                .Register(Assembly.GetExecutingAssembly())
                .Done();

            Run(system);

            Console.WriteLine("Press any key to terminate ...");
            Console.ReadKey(true);

            system.Dispose();            
            Environment.Exit(0);
        }

        static async void Run(IActorSystem system)
        {
            var rwx = system.ActorOf<ReaderWriterLock>("rw-x");
            await rwx.Ask<int>(new Read()); // warm-up

            var writes = new List<Task>
            {
                rwx.Tell(new Write {Value = 1, Delay = TimeSpan.FromMilliseconds(800)}),
                rwx.Tell(new Write {Value = 2, Delay = TimeSpan.FromMilliseconds(200)}),
            };

            var cts = new CancellationTokenSource();
            var reads = new List<int>();

            Console.Write("\nReads: ");
            
            int left = Console.CursorLeft;
            int top = Console.CursorTop;

            Task.Run(async () =>
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    reads.Add(await rwx.Ask<int>(new Read()));
                    Console.SetCursorPosition(left, top);
                    Console.Write(reads.Count);
                }
            },
            cts.Token).Ignore();

            await Task.WhenAll(writes);
            cts.Cancel();

            Debug.Assert(reads.Count > writes.Count * 100,
                "Should actually serve reads in parallel, while there are slow sequential writes in flight");

            Debug.Assert(reads.OrderBy(x => x).SequenceEqual(reads),
                "All readers should see consistently incrementing sequence, despite that 2nd write is faster. Writes are queued");

            Debug.Assert(reads.Distinct().SequenceEqual(new[] {1, 2}),
                "Should see all changes of the write sequence");
        }
    }
}
