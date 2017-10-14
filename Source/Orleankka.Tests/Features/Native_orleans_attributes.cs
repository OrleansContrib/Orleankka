using System;
using System.Reflection;

using NUnit.Framework;

using Orleans.CodeGeneration;
using Orleans.Concurrency;
using Orleans.MultiCluster;
using Orleans.Placement;
using Orleans.Providers;
using Orleans.Runtime;

namespace Orleankka.Features
{
    namespace Native_orleans_attributes
    {
        using Core;
        using Testing;

        [Version(22)]
        public interface ITestVersionActor : IActor {}
        public class TestVersionActor : Actor, ITestVersionActor {}

        [StatelessWorker]
        public class TestDefaultStatelessWorkerActor : Actor {}

        [StatelessWorker(4)]
        public class TestParameterizedStatelessWorkerActor : Actor {}

        [StorageProvider]
        public class TestDefaultStorageProviderActor : Actor {}

        [GlobalSingleInstance]
        public class TestGlobalSingleInstanceActor : Actor {}

        [OneInstancePerCluster]
        public class TestOneInstancePerClusterActor : Actor {}

        [RandomPlacement]
        public class TestRandomPlacementActor : Actor {}

        [PreferLocalPlacement]
        public class TestPreferLocalPlacementActor : Actor {}

        [ActivationCountBasedPlacement]
        public class TestActivationCountBasedPlacementActor : Actor {}

        [HashBasedPlacement]
        public class TestHashBasedPlacementActor : Actor {}

        [AttributeUsage(AttributeTargets.Class)]
        public class CustomPlacementAttribute : PlacementAttribute
        {
            public Type PType { get; }
            public ConsoleColor PEnum { get; }
            public string PString { get; }
            public bool PBool { get; }
            public short PShort { get; }
            public int PInt { get; }
            public long PLong { get; }
            public double PDouble { get; }
            public float PFloat { get; }

            public CustomPlacementAttribute(Type pType, ConsoleColor pEnum, string pString, bool pBool, short pShort, int pInt, long pLong, double pDouble, float pFloat)
                : base(new HashBasedPlacement())
            {
                PType = pType;
                PEnum = pEnum;
                PString = pString;
                PBool = pBool;
                PShort = pShort;
                PInt = pInt;
                PLong = pLong;
                PDouble = pDouble;
                PFloat = pFloat;
            }
        }

        [CustomPlacement(typeof(string), ConsoleColor.Blue, "test", true, 1, 2, 3, 4.4d, 5.6F)]
        public class TestCustomPlacementActor : Actor {}

        [TestFixture]
        [RequiresSilo]
        public class Tests
        {
            static TAttribute AssertHasCustomAttribute<TActor, TAttribute>() where TActor : Actor where TAttribute : Attribute => 
                AssertHasCustomAttribute<TAttribute>(ActorType.Of<TActor>().Grain);

            static TAttribute AssertHasCustomAttribute<TAttribute>(Type type) where TAttribute : Attribute
            {
                var attribute = type.GetCustomAttribute<TAttribute>();
                Assert.That(attribute, Is.Not.Null);
                return attribute;
            }

            [Test]
            public void Version_attribute()
            {
                var @interface = ActorInterface.Of<TestVersionActor>();
                var attribute = AssertHasCustomAttribute<VersionAttribute>(@interface.Grain);
                Assert.That(attribute.Version, Is.EqualTo(22));
            }

            [Test]
            public void StatelessWorker_attribute()
            {
                var attribute = AssertHasCustomAttribute<TestDefaultStatelessWorkerActor, StatelessWorkerAttribute>();
                Assert.That(attribute.MaxLocalWorkers, Is.EqualTo(new StatelessWorkerAttribute().MaxLocalWorkers));

                attribute = AssertHasCustomAttribute<TestParameterizedStatelessWorkerActor, StatelessWorkerAttribute>();
                Assert.That(attribute.MaxLocalWorkers, Is.EqualTo(4));
            }

            [Test]
            public void StorageProvider_attribute()
            {
                var attribute = AssertHasCustomAttribute<TestDefaultStorageProviderActor, StorageProviderAttribute>();
                Assert.That(attribute.ProviderName, Is.EqualTo(new StorageProviderAttribute().ProviderName));
            }

            [Test]
            public void MultiCluster_attributes()
            {
                AssertHasCustomAttribute<TestGlobalSingleInstanceActor, GlobalSingleInstanceAttribute>();
                AssertHasCustomAttribute<TestOneInstancePerClusterActor, OneInstancePerClusterAttribute>();
            }

            [Test]
            public void Placement_attributes()
            {
                 AssertHasCustomAttribute<TestRandomPlacementActor, RandomPlacementAttribute>();
                 AssertHasCustomAttribute<TestPreferLocalPlacementActor, PreferLocalPlacementAttribute>();
                 AssertHasCustomAttribute<TestActivationCountBasedPlacementActor, ActivationCountBasedPlacementAttribute>();
                 AssertHasCustomAttribute<TestHashBasedPlacementActor, HashBasedPlacementAttribute>();
            }

            [Test]
            public void Custom_placement_attribute()
            {
                var attribute = AssertHasCustomAttribute<TestCustomPlacementActor, CustomPlacementAttribute>();
                Assert.That(attribute.PType, Is.EqualTo(typeof(string)));
                Assert.That(attribute.PEnum, Is.EqualTo(ConsoleColor.Blue));
                Assert.That(attribute.PString, Is.EqualTo("test"));
                Assert.That(attribute.PBool, Is.EqualTo(true));
                Assert.That(attribute.PShort, Is.EqualTo(1));
                Assert.That(attribute.PInt, Is.EqualTo(2));
                Assert.That(attribute.PLong, Is.EqualTo(3));
                Assert.That(attribute.PDouble, Is.EqualTo(4.4d));
                Assert.That(attribute.PFloat, Is.EqualTo(5.6F));
            }
        }
    }
}