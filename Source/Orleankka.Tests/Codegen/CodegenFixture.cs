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
                Is.EqualTo(8));
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
        public void From_non_attributed_type()
        {
            AssertContains(ActorEndpointDeclaration.From(typeof(RegularActor)), ActivationAttribute.Actor);
        }

        [Test]
        public void From_unordered_worker()
        {
            AssertContains(ActorEndpointDeclaration.From(typeof(UnorderedWorker)),
                ActivationAttribute.Worker,
                DeliveryAttribute.Unordered);
        }

        [Test]
        public void From_actor_with_prefer_local_placement()
        {
            AssertContains(ActorEndpointDeclaration.From(typeof(PreferLocalActor)),
                ActivationAttribute.Actor,
                PlacementAttribute.PreferLocal);
        }

        static void AssertContains(ActorEndpointDeclaration declaration, params ActorEndpointAttribute[] attributes)
        {
            Assert.That(declaration, Is.EqualTo(ActorEndpointDeclaration.Mix(attributes)));
        }

        class RegularActor 
        {}
        
        [Worker(Delivery.Unordered)]
        class UnorderedWorker 
        {}

        [Actor(Placement.PreferLocal)]
        class PreferLocalActor : Actor
        {}
    }
}
