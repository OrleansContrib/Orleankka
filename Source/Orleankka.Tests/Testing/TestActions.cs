using System;

using Microsoft.WindowsAzure.Storage;

using NUnit.Framework;
using NUnit.Framework.Interfaces;

using Orleans;
using Orleans.Hosting;
using Orleans.Runtime.Configuration;

using Orleankka.Testing;
[assembly: TeardownSilo]

namespace Orleankka.Testing
{
    using Client;
    using Cluster;

    using Features.Intercepting_requests;

    [AttributeUsage(AttributeTargets.Class)]
    public class RequiresSiloAttribute : TestActionAttribute
    {
        public override void BeforeTest(ITest test)
        {
            if (!test.IsSuite)
                return;

            if (TestActorSystem.Instance != null)
                return;

            var sc = ClusterConfiguration.LocalhostPrimarySilo();            
            sc.DefaultKeepAliveTimeout(TimeSpan.FromMinutes(1));
            
            sc.AddMemoryStorageProvider();
            sc.AddMemoryStorageProvider("PubSubStore");

            sc.AddSimpleMessageStreamProvider("sms");
            sc.AddAzureQueueStreamProviderV2("aqp",
                $"{CloudStorageAccount.DevelopmentStorageAccount}", 
                clusterId: "test");

            sc.Globals.DataConnectionStringForReminders = $"{CloudStorageAccount.DevelopmentStorageAccount}";
            sc.Globals.ReminderServiceType = GlobalConfiguration.ReminderServiceProviderType.AzureTable;

            var sb = new SiloHostBuilder()
                .UseConfiguration(sc)
                .ConfigureApplicationParts(x => x
                    .AddApplicationPart(GetType().Assembly)
                    .WithCodeGeneration())
                .ConfigureOrleankka(x => x
                    .ActorMiddleware(typeof(TestActorBase), new TestActorMiddleware()));

            var host = sb.Build();
            host.StartAsync().Wait();

            var cc = ClientConfiguration.LocalhostSilo();
            cc.AddSimpleMessageStreamProvider("sms");
            cc.AddAzureQueueStreamProviderV2("aqp",
                $"{CloudStorageAccount.DevelopmentStorageAccount}", 
                clusterId: "test");

            var cb = new ClientBuilder()
                .UseConfiguration(cc)
                .ConfigureApplicationParts(x => x
                    .AddApplicationPart(GetType().Assembly)
                    .WithCodeGeneration())
                .ConfigureOrleankka(x => x
                    .ActorRefMiddleware(new TestActorRefMiddleware()));

            var client = cb.Build();
            client.Connect().Wait();

            TestActorSystem.Host = host;
            TestActorSystem.Client = client;
            TestActorSystem.Instance = client.ActorSystem();
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

            TestActorSystem.Client.Close().Wait();
            TestActorSystem.Client.Dispose();
            TestActorSystem.Host.StopAsync().Wait();
            TestActorSystem.Host.Dispose();
            
            TestActorSystem.Client = null;
            TestActorSystem.Host = null;
            TestActorSystem.Instance = null;
        }
    }
}
