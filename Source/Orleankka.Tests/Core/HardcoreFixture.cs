using System;
using System.Linq;
using System.Threading.Tasks;

using NUnit.Framework;

using Orleans;
using Orleans.Concurrency;

namespace Orleankka.Core
{
    using Hardcore;

    [TestFixture]
    public class HardcoreFixture
    {
        [Test]
        public void Check_mixed_combinations()
        {
            Assert.That(Mixer.AllBlends.Count(), Is.EqualTo(128));
            Assert.That(Mixer.AllowedBlends.Count(), Is.EqualTo(50));
        }

        [Test, Explicit]
        public void Check_mixing_visually()
        {
            Console.Write("All blends: ");
            Console.WriteLine(Mixer.AllBlends.Count());
            
            foreach (var blend in Mixer.AllBlends)
                Console.WriteLine(blend);

            Console.WriteLine("Allowed blends: ");
            Console.WriteLine(Mixer.AllowedBlends.Count());

            foreach (var blend in Mixer.AllowedBlends)
            {
                Console.WriteLine(blend);
                
                Print("-Class-",     blend.GetClassAttributesString());
                Print("-Interface-", blend.GetInterfaceAttributeString());
                Print("-Tell-",      blend.GetTellMethodAttributesString());
                Print("-Ask-",       blend.GetAskMethodAttributeString());
            }
        }

        static void Print(string label, string text)
        {
            if (text == String.Empty)
                return;

            Console.Write(label);
            Console.WriteLine(text);
        }

        [Test]
        public void Blend_from_non_attributed_type()
        {
            AssertContains(Blend.From(typeof(RegularSingleton)), Activation.Singleton);
        }

        [Test]
        public void Blend_from_unordered_stateless_worker()
        {
            AssertContains(Blend.From(typeof(UnorderedStatelessWorker)),
                Activation.StatelessWorker,
                Delivery.Unordered);
        }

        [Test]
        public void Blend_from_singleton_with_tell_method_attributed()
        {
            AssertContains(Blend.From(typeof(TellInterleave)),
                Activation.Singleton,
                Interleave.Tell);
        }

        [Test]
        public void Blend_from_worker_with_both_methods_attributed()
        {
            AssertContains(Blend.From(typeof(WorkerWithInterleave)), 
                Activation.StatelessWorker, 
                Interleave.Both);
        }

        static void AssertContains(Blend blend, params Flavor[] flavors)
        {
            Assert.That(blend, Is.EqualTo(Blend.Mix(flavors)));
        }

        class RegularSingleton {}
        
        [Unordered, StatelessWorker] 
        class UnorderedStatelessWorker {}

        class TellInterleave : Actor
        {
            [AlwaysInterleave]
            public override Task OnTell(object message)
            {
                return TaskDone.Done;
            }
        }

        [StatelessWorker] 
        class WorkerWithInterleave : Actor
        {
            [AlwaysInterleave]
            public override Task OnTell(object message)
            {
                return TaskDone.Done;
            }

            [AlwaysInterleave]
            public override Task<object> OnAsk(object message)
            {
                return Task.FromResult(new object());
            }
        }
    }
}
