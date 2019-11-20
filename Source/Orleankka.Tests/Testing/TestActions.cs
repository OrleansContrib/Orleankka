using System;

using NUnit.Framework;
using NUnit.Framework.Interfaces;

using Orleankka.Features.Stateful_actors;
using Orleans.Runtime.Configuration;

using Orleankka.Testing;
[assembly: TeardownSilo]

namespace Orleankka.Testing
{
    using Cluster;
    using Playground;
    using Utility;
    using Features.Intercepting_requests;

    using Orleans.Hosting;
    using Orleans.Providers.Streams.AzureQueue;

    [AttributeUsage(AttributeTargets.Class)]
    public class RequiresSiloAttribute : TestActionAttribute
    {
        public override void BeforeTest(ITest test)
        {
            if (!test.IsSuite)
                return;

            if (TestActorSystem.Instance != null)
                return;

            using (Trace.Execution("Full system startup"))
            {
                var system = ActorSystem.Configure()
                    .Playground()                    
                    .Cluster(x =>
                    {
                        x.Configuration.Globals.CollectionQuantum = TimeSpan.FromSeconds(1);
                        x.Configuration.DefaultKeepAliveTimeout(TimeSpan.FromMinutes(1));
                        x.Configuration.Globals.RegisterStorageProvider<TestActorStorageProvider>("Test");

                        x.ActorInvoker("test_actor_interception", new TestActorInterceptionInvoker());
                        x.ActorInvoker("test_stream_interception", new TestStreamInterceptionInvoker());

                        x.Builder(b =>
                        {
                            b.UseAzureTableReminderService("UseDevelopmentStorage=true");
                            b.AddAzureQueueStreams<AzureQueueDataAdapterV2>("aqp", options =>
                            {
                                options.Configure(c => c.ConnectionString = "UseDevelopmentStorage=true");
                            });

                            b.AddStartupTask(Features.Autorun_actors.StartupTask.Run);
                        });

                        x.RegisterPersistentStreamProviders("aqp");
                    })
                    .Client(x =>
                    {
                        x.ActorRefInvoker(new TestActorRefInvoker());

                        x.Builder(b =>
                        {
                            b.AddAzureQueueStreams<AzureQueueDataAdapterV2>("aqp", options =>
                            {
                                options.Configure(c => c.ConnectionString = "UseDevelopmentStorage=true");
                            });
                        });
                    })
                    .Assemblies(GetType().Assembly);

                TestActorSystem.Instance = system.Done();
                TestActorSystem.Instance.Start().Wait();
            }
        }
    }

    public class TeardownSiloAttribute : TestActionAttribute
    {
        public override void AfterTest(ITest test)
        {
            if (!test.IsSuite)
                return;

            if (TestActorSystem.Instance == null)
                return;

            TestActorSystem.Instance.Stop().Wait();
            TestActorSystem.Instance.Dispose();
            TestActorSystem.Instance = null;
        }
    }

    public interface IGotcha : Orleans.IGrainWithGuidKey
    {}
}