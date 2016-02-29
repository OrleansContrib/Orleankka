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
    [AttributeUsage(AttributeTargets.Class)]
    public class RequiresSiloAttribute : TestActionAttribute
    {
        public bool Fresh;
        public int DefaultKeepAliveTimeoutInMinutes = 1;
        public bool EnableAzureQueueStreamProvider = false;
        public string RegisterAssembly = null;

        public override void BeforeTest(TestDetails details)
        {
            if (!details.IsSuite)
                return;

            //if (Fresh)
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
                    .DefaultKeepAliveTimeout(TimeSpan.FromMinutes(DefaultKeepAliveTimeoutInMinutes)));                

            var assembly = string.IsNullOrEmpty(RegisterAssembly)
                ? GetType().Assembly
                : AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == RegisterAssembly);

            system = system.Register(assembly);

            if (EnableAzureQueueStreamProvider)
            {
                system.Register<AzureQueueStreamProvider>("aqp", new Dictionary<string, string>
                {
                    {"DataConnectionString", "UseDevelopmentStorage=true"},
                    {"DeploymentId", "test"},
                });
            }

            TestActorSystem.Instance = system.Done();
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
