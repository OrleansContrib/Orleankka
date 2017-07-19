using System;
using System.Reflection;

using NUnit.Framework;

using Orleans.CodeGeneration;
using Orleans.Concurrency;
using Orleans.Providers;

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
        }
    }
}