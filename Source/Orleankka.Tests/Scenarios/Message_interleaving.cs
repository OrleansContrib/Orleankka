using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;
using Orleans;

namespace Orleankka.Scenarios
{
    [TestFixture]
    public class Message_interleaving
    {
        static readonly IActorSystem system = ActorSystem.Instance;

        [Test]
        public async void Should_queue_writes_but_interleave_reads()
        {
            var rwx = system.ActorOf<ReaderWriterLockActor>("rw-x");

            var writes = new List<Task>
            {
                rwx.Tell(new Write {Value = 1, Delay = TimeSpan.FromMilliseconds(200)}),
                rwx.Tell(new Write {Value = 2, Delay = TimeSpan.FromMilliseconds(100)})
            };

            var cts = new CancellationTokenSource();
            var reads = new List<long>();

            Task.Run(async () =>
            {
                while (!cts.Token.IsCancellationRequested)
                    reads.Add(await rwx.Ask<long>(new Read()));
            }, 
            cts.Token).Ignore();

            await Task.WhenAll(writes);
            cts.Cancel();

            Assert.That(reads.Count, Is.AtLeast(writes.Count * 100),
                "Should actually serve reads in parallel, while there are slow sequential writes in flight");
            
            Assert.That(reads.OrderBy(x => x).ToArray(), Is.EqualTo(reads), 
                "All readers should see consistently incrementing sequence, despite that 2nd write is faster. Writes are queued");

            Assert.That(reads.Distinct(), Is.EquivalentTo(new[] {1, 2}), 
                "Should see all changes of the write sequence");
        }
    }

    class Write : Command
    {
        public int Value;
        public TimeSpan Delay;
    }

    class Read : Query<int> 
    {}

    [Reentrant(typeof(Read))]
    class ReaderWriterLockActor : Actor
    {
        long currentValue;

        public override Task<object> OnReceive(object message)
        {
            return this.On((dynamic) message);
        }

        public async Task<object> On(Write req)
        {
            currentValue = req.Value;
            await Task.Delay(req.Delay);
            return Done();
        }
        
        public Task<object> On(Read req)
        {
            return Result(currentValue);
        }
    }
}
