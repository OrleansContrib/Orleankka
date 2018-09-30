using System;
using System.Net;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;
using NUnit.Framework.Interfaces;

using Orleans;
using Orleans.Hosting;
using Orleans.Configuration;
using Orleans.Runtime;
using Orleans.Storage;

using Orleankka.Testing;
[assembly: TeardownSilo]

namespace Orleankka.Testing
{
    using Client;
    using Cluster;

    using Features.Intercepting_requests;

    using ClusterOptions = Orleans.Configuration.ClusterOptions;

    [AttributeUsage(AttributeTargets.Class)]
    public class RequiresSiloAttribute : TestActionAttribute
    {
        const string DemoClusterId = "localhost-demo";
        const string DemoServiceId = "localhost-demo-service";

        const int LocalhostSiloPort = 11111;
        const int LocalhostGatewayPort = 30000;
        static readonly IPAddress LocalhostSiloAddress = IPAddress.Loopback;

        public override void BeforeTest(ITest test)
        {
            if (!test.IsSuite)
                return;

            if (TestActorSystem.Instance != null)
                return;

            var sb = new SiloHostBuilder()
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = DemoClusterId;
                    options.ServiceId = DemoServiceId;
                })
                .UseDevelopmentClustering(options => options.PrimarySiloEndpoint = new IPEndPoint(LocalhostSiloAddress, LocalhostSiloPort))
                .ConfigureEndpoints(LocalhostSiloAddress, LocalhostSiloPort, LocalhostGatewayPort)
                .AddMemoryGrainStorageAsDefault()
                .AddMemoryGrainStorage("PubSubStore")
                .AddSimpleMessageStreamProvider("sms")
                .UseInMemoryReminderService()
                .ConfigureServices(services =>
                {
                    services.Configure<GrainCollectionOptions>(options => options.CollectionAge = TimeSpan.FromMinutes(1));
                })
                .ConfigureApplicationParts(x => x
                    .AddApplicationPart(GetType().Assembly)
                    .AddApplicationPart(typeof(MemoryGrainStorage).Assembly)
                    .WithCodeGeneration())
                .UseOrleankka(x => x
                    .ActorMiddleware(typeof(TestActorBase), new TestActorMiddleware()));

            var host = sb.Build();
            host.StartAsync().Wait();

            var cb = new ClientBuilder()
                .Configure<ClusterOptions>(options => options.ClusterId = DemoClusterId)
                .UseStaticClustering(options => options.Gateways.Add(new IPEndPoint(LocalhostSiloAddress, LocalhostGatewayPort).ToGatewayUri()))
                .AddSimpleMessageStreamProvider("sms")
                .ConfigureApplicationParts(x => x
                    .AddApplicationPart(GetType().Assembly)
                    .WithCodeGeneration())
                .UseOrleankka(x => x
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
