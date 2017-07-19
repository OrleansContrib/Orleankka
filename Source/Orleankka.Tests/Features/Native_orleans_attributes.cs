using System.Reflection;

using NUnit.Framework;

using Orleans.CodeGeneration;
using Orleans.Concurrency;

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

        [TestFixture]
        [RequiresSilo]
        public class Tests
        {
            [Test]
            public void Version_attribute()
            {
                var @interface = ActorInterface.Of<TestVersionActor>();
                var attribute = @interface.Grain.GetCustomAttribute<VersionAttribute>();
                Assert.That(attribute, Is.Not.Null);
                Assert.That(attribute.Version, Is.EqualTo(22));
            }

            [Test]
            public void StatelessWorker_attribute()
            {
                var type = ActorType.Of<TestDefaultStatelessWorkerActor>();
                var attribute = type.Grain.GetCustomAttribute<StatelessWorkerAttribute>();
                Assert.That(attribute, Is.Not.Null);
                Assert.That(attribute.MaxLocalWorkers, Is.EqualTo(new StatelessWorkerAttribute().MaxLocalWorkers));

                type = ActorType.Of<TestParameterizedStatelessWorkerActor>();
                attribute = type.Grain.GetCustomAttribute<StatelessWorkerAttribute>();
                Assert.That(attribute, Is.Not.Null);
                Assert.That(attribute.MaxLocalWorkers, Is.EqualTo(4));
            }
        }
    }
}