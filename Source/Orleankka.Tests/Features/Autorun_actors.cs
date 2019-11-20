using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Orleankka.Features
{
    namespace Autorun_actors
    {
        using Meta;
        using Testing;

        [Serializable]
        public class Autorun : Message
        {
            public static Autorun Message = new Autorun();
        }

        [Serializable]
        public class WhenActivated : Query<DateTime>
        {}

        public interface ITestActor : IActor
        {}

        public class TestActor : Actor, ITestActor
        {
            DateTime activated;

            public override Task OnActivate()
            {
                activated = DateTime.Now;
                return base.OnActivate();
            }

            void On(Autorun _) {}
            DateTime On(WhenActivated _) => activated;
        }

        public class StartupTask
        {
            public static async Task Run(IServiceProvider services, CancellationToken _)
            {
                var system = services.GetService<IActorSystem>();

                var runs = new List<Task>
                {
                    system.ActorOf<ITestActor>("a1").Tell(Autorun.Message),
                    system.ActorOf<ITestActor>("a2").Tell(Autorun.Message)
                };

                await Task.WhenAll(runs);
            }
        }

        [TestFixture, RequiresSilo]
        [Category("Slow")]
        public class Tests
        {
            IActorSystem system;

            [SetUp]
            public void SetUp()
            {
                system = TestActorSystem.Instance;
            }

            [Test]
            public async Task Autorun_actors_could_be_simulated_with_silo_startup_task()
            {
                // wait a bit
                Thread.Sleep(TimeSpan.FromSeconds(5));

                var a1 = system.ActorOf<ITestActor>("a1");
                var a2 = system.ActorOf<ITestActor>("a1");

                Assert.That(await a1.Ask(new WhenActivated()), Is.LessThan(DateTime.Now.AddSeconds(-2)));
                Assert.That(await a2.Ask(new WhenActivated()), Is.LessThan(DateTime.Now.AddSeconds(-2)));
            }
        }
    }
}