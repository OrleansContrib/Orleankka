using System;
using System.Collections.Generic;

using NUnit.Framework;

using Orleankka.Cluster;
using Orleankka.Features.Intercepting_requests;
using Orleankka.Playground;
using Orleankka.Testing;
using Orleans.Providers.Streams.AzureQueue;

[assembly: TeardownSilo]

namespace Orleankka.Testing
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RequiresSiloAttribute : TestActionAttribute
    {
        public bool Fresh;
        public int DefaultKeepAliveTimeoutInMinutes = 1;
        public bool EnableAzureQueueStreamProvider = false;

        public override void BeforeTest(TestDetails details)
        {
            if (!details.IsSuite)
                return;

            if (Fresh)
                TeardownExisting();

            StartNew();
        }

        static void TeardownExisting()
        {
            if (TestActorSystem.Instance == null)
                return;

            TestActorSystem.Instance.Dispose();
            TestActorSystem.Instance = null;
        }

        void StartNew()
        {
            if (TestActorSystem.Instance != null)
                return;

            var system = ActorSystem.Configure()
                .Playground()
                .UseInMemoryPubSubStore()
                .TweakCluster(cfg => cfg
                    .DefaultKeepAliveTimeout(TimeSpan.FromMinutes(DefaultKeepAliveTimeoutInMinutes)))
                .Assemblies(GetType().Assembly);

            if (EnableAzureQueueStreamProvider)
            {
                system.StreamProvider<AzureQueueStreamProvider>("aqp", new Dictionary<string, string>
                {
                    {"DataConnectionString", "UseDevelopmentStorage=true"},
                    {"DeploymentId", "test"},
                });
            }

            system.Interceptor<TestInterceptor>();

            TestActorSystem.Instance = system.Done();
            TestActorSystem.Instance.Start();
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

            TestActorSystem.Instance.Dispose();
            TestActorSystem.Instance = null;
        }
    }
}
