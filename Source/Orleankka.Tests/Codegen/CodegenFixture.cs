using System;
using System.Linq;

using NUnit.Framework;

namespace Orleankka.Codegen
{
    [TestFixture]
    public class CodegenFixture
    {
        [Test]
        public void Check_mixed_combinations()
        {
            Assert.That(ActorEndpointDeclaration.AllPossibleDeclarations.Count(), 
                Is.EqualTo(32));
        }

        [Test, Explicit]
        public void Check_mixing_visually()
        {
            Console.WriteLine("All possible blends: ");
            Console.WriteLine(ActorEndpointDeclaration.AllPossibleDeclarations.Count());

            foreach (var decl in ActorEndpointDeclaration.AllPossibleDeclarations)
            {
                Console.WriteLine(decl);
                
                Print("-Class-",     decl.GetClassAttributesString());
                Print("-Interface-", decl.GetInterfaceAttributeString());
                Print("-Tell-",      decl.GetTellMethodAttributesString());
                Print("-Ask-",       decl.GetAskMethodAttributeString());
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
            AssertContains(ActorEndpointDeclaration.From(typeof(RegularSingleton)), ActivationAttribute.Actor);
        }

        [Test]
        public void Blend_from_unordered_stateless_worker()
        {
            AssertContains(ActorEndpointDeclaration.From(typeof(UnorderedStatelessWorker)),
                ActivationAttribute.Worker,
                DeliveryAttribute.Unordered);
        }

        [Test]
        public void Blend_from_singleton_with_prefer_local_placement_and_tell_interleaved()
        {
            AssertContains(ActorEndpointDeclaration.From(typeof(PreferLocalTellInterleave)),
                ActivationAttribute.Actor,
                PlacementAttribute.PreferLocal,
                ConcurrencyAttribute.TellInterleave);
        }

        [Test]
        public void Blend_from_reentrant_worker()
        {
            AssertContains(ActorEndpointDeclaration.From(typeof(ReentrantWorker)), 
                ActivationAttribute.Worker, 
                ConcurrencyAttribute.Reentrant);
        }

        static void AssertContains(ActorEndpointDeclaration declaration, params ActorEndpointAttribute[] attributes)
        {
            Assert.That(declaration, Is.EqualTo(ActorEndpointDeclaration.Mix(attributes)));
        }

        class RegularSingleton 
        {}
        
        [Worker.Configuration(delivery: Delivery.Unordered)]
        class UnorderedStatelessWorker 
        {}

        [Actor.Configuration(
            placement: Placement.PreferLocal, 
            concurrency: Concurrency.TellInterleave)]
        class PreferLocalTellInterleave : Actor
        {}

        [Worker.Configuration(Concurrency.Reentrant)]
        class ReentrantWorker : Actor
        {}
    }
}
