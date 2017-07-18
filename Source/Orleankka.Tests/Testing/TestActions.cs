using System;
using System.Collections.Generic;

using NUnit.Framework;

using Orleans.Storage;
using Orleans.Providers.Streams.AzureQueue;

using Orleankka.Testing;
[assembly: TeardownSilo]

namespace Orleankka.Testing
{
    using Cluster;
    using Playground;
    using Utility;
    using Features.Intercepting_requests;

    [AttributeUsage(AttributeTargets.Class)]
    public class RequiresSiloAttribute : TestActionAttribute
    {
        public override void BeforeTest(TestDetails details)
        {
            if (!details.IsSuite)
                return;

            if (TestActorSystem.Instance != null)
                return;

            using (Trace.Execution("Full system startup"))
            {
                var system = ActorSystem.Configure()
                    .Playground()
                    .UseInMemoryPubSubStore()
                    .StreamProvider<AzureQueueStreamProviderV2>("aqp", new Dictionary<string, string>
                    {
                        {"DataConnectionString", "UseDevelopmentStorage=true"},
                        {"DeploymentId", "test"},
                    })
                    .TweakCluster(cfg =>
                    {
                        cfg.DefaultKeepAliveTimeout(TimeSpan.FromMinutes(1));
                        cfg.Globals.RegisterStorageProvider<MemoryStorage>("MemoryStore");
                    })
                    .ActorInvoker("test_actor_interception", new TestActorInterceptionInvoker())
                    .ActorInvoker("test_stream_interception", new TestStreamInterceptionInvoker())
                    .Assemblies(GetType().Assembly);

                TestActorSystem.Instance = system.Done();
                TestActorSystem.Instance.Start().Wait();
            }
        }
    }

    public class TeardownSiloAttribute : TestActionAttribute
    {
        public override void AfterTest(TestDetails details)
        {
            if (!details.IsSuite)
                return;

            if (TestActorSystem.Instance == null)
                return;

            TestActorSystem.Instance.Stop(true).Wait();
            TestActorSystem.Instance.Dispose();
            TestActorSystem.Instance = null;
        }
    }
}
