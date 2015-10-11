using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using Orleankka.Cluster;
using Orleankka.Playground;
using Orleankka.Testing;

using Orleans.Providers.Streams.AzureQueue;

[assembly: TeardownSilo]

namespace Orleankka.Testing
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class RequiresSiloAttribute : TestActionAttribute
    {
        public bool Fresh;
        public int DefaultKeepAliveTimeoutInMinutes = 1;

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

            TestActorSystem.Instance = ActorSystem.Configure()
                .Playground()
                .UseInMemoryPubSubStore()
                .TweakCluster(cfg => cfg
                    .DefaultKeepAliveTimeout(TimeSpan.FromMinutes(DefaultKeepAliveTimeoutInMinutes)))
                .Register(GetType().Assembly)
                .Register<AzureQueueStreamProvider>("aqp", new Dictionary<string, string>
                {
                    {"DataConnectionString", "UseDevelopmentStorage=true"},
                    {"DeploymentId", "test"},
                })
                .Done();
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
