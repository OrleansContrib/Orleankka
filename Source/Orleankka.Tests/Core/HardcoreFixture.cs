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
            Blend blend = Blend.From(typeof(RegularSingleton));

            Assert.That(blend.Flavors, Contains.Item(Activation.Singleton));
            Assert.That(blend.Flavors, Contains.Item(Concurrency.Default));
            Assert.That(blend.Flavors, Contains.Item(Placement.Default));
            Assert.That(blend.Flavors, Contains.Item(Delivery.Default));
            Assert.That(blend.Flavors, Contains.Item(Interleave.Default));
        }

        [Test]
        public void Blend_from_unordered_stateless_worker()
        {
            Blend blend = Blend.From(typeof(UnorderedStatelessWorker));

            Assert.That(blend.Flavors, Contains.Item(Activation.StatelessWorker));
            Assert.That(blend.Flavors, Contains.Item(Concurrency.Default));
            Assert.That(blend.Flavors, Contains.Item(Placement.Default));
            Assert.That(blend.Flavors, Contains.Item(Delivery.Unordered));
            Assert.That(blend.Flavors, Contains.Item(Interleave.Default));
        }

        [Test]
        public void Blend_from_singleton_with_tell_method_attributed()
        {
            Blend blend = Blend.From(typeof(TellInterleave));

            Assert.That(blend.Flavors, Contains.Item(Activation.Singleton));
            Assert.That(blend.Flavors, Contains.Item(Concurrency.Default));
            Assert.That(blend.Flavors, Contains.Item(Placement.Default));
            Assert.That(blend.Flavors, Contains.Item(Delivery.Default));
            Assert.That(blend.Flavors, Contains.Item(Interleave.Tell));
        }

        [Test]
        public void Blend_from_worker_with_both_methods_attributed()
        {
            Blend blend = Blend.From(typeof(WorkerWithInterleave));

            Assert.That(blend.Flavors, Contains.Item(Activation.StatelessWorker));
            Assert.That(blend.Flavors, Contains.Item(Concurrency.Default));
            Assert.That(blend.Flavors, Contains.Item(Placement.Default));
            Assert.That(blend.Flavors, Contains.Item(Delivery.Default));
            Assert.That(blend.Flavors, Contains.Item(Interleave.Both));
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
