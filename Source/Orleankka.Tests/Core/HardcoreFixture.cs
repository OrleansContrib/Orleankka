using System;
using System.Linq;

using NUnit.Framework;

namespace Orleankka.Core
{
    using Hardcore;

    [TestFixture]
    public class HardcoreFixture
    {
        [Test]
        public void Check_mixed_combinations()
        {
            Assert.That(Mixer.AllBlends.Count(), Is.EqualTo(64));
            Assert.That(Mixer.AllowedBlends.Count(), Is.EqualTo(40));
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
        public void Blend_from_singleton_with_prefer_local_placement_and_tell_interleaved()
        {
            AssertContains(Blend.From(typeof(PreferLocalTellInterleave)),
                Activation.Singleton,
                Placement.PreferLocal,
                Concurrency.TellInterleave);
        }

        [Test]
        public void Blend_from_reentrant_worker()
        {
            AssertContains(Blend.From(typeof(ReentrantWorker)), 
                Activation.StatelessWorker, 
                Concurrency.Reentrant);
        }

        static void AssertContains(Blend blend, params Flavor[] flavors)
        {
            Assert.That(blend, Is.EqualTo(Blend.Mix(flavors)));
        }

        class RegularSingleton 
        {}
        
        [ActorConfiguration(ActivationKind.StatelessWorker, delivery: DeliveryKind.Unordered)]
        class UnorderedStatelessWorker 
        {}

        [ActorConfiguration(placement: PlacementKind.PreferLocal, concurrency: ConcurrencyKind.TellInterleave)]
        class PreferLocalTellInterleave : Actor
        {}

        [ActorConfiguration(ActivationKind.StatelessWorker, concurrency: ConcurrencyKind.Reentrant)]
        class ReentrantWorker : Actor
        {}
    }
}
