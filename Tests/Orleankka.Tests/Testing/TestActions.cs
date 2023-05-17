using System;
using System.Linq;
using System.Net;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;
using NUnit.Framework.Interfaces;

using Orleans;
using Orleans.Hosting;
using Orleans.Configuration;
using Orleans.Storage;
using Orleans.Runtime;

using Orleankka.Testing;
using Orleankka.Cluster;
using Orleankka.Features.Intercepting_requests;
using Orleankka.Legacy.Cluster;

[assembly: TeardownSilo]

namespace Orleankka.Testing
{
    using Features.State_persistence;

    using Orleans.Serialization;

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

            var sb = new HostBuilder()
                .UseOrleans((ctx, sb) => sb
                    .Configure<ClusterOptions>(options =>
                    {
                        options.ClusterId = DemoClusterId;
                        options.ServiceId = DemoServiceId;
                    })
                    .UseDevelopmentClustering(options => options.PrimarySiloEndpoint = new IPEndPoint(LocalhostSiloAddress, LocalhostSiloPort))
                    .ConfigureEndpoints(LocalhostSiloAddress, LocalhostSiloPort, LocalhostGatewayPort)
                    .AddMemoryGrainStorageAsDefault()
                    .AddMemoryGrainStorage("PubSubStore")
                    .UseInMemoryReminderService()
                    .ConfigureServices(services =>
                    {
                        services.AddSingletonNamedService<IGrainStorage>("test", (sp, name) => new TestStorageProvider(name));
                        services.Configure<GrainCollectionOptions>(options => options.CollectionAge = TimeSpan.FromMinutes(1.1));
                        
                        services.Configure<DispatcherOptions>(o =>
                        {
                            var customConventions = Dispatcher.DefaultHandlerNamingConventions.Concat(new[]{"OnFoo"});
                            o.HandlerNamingConventions = customConventions.ToArray();
                        });

                        services.AddSingleton<IActorRefMiddleware>(s => new TestActorRefMiddleware());
                        services.AddSingleton<IActorMiddleware>(s => new TestActorMiddleware());

                        services.AddSerializer(serializerBuilder =>
                        {
                            serializerBuilder.AddNewtonsoftJsonSerializer(
                                isSupported: type => typeof(Meta.Message).IsAssignableFrom(type), 
                                options => options.Configure(c =>
                                {
                                    var system = new Lazy<IActorSystem>(()=> TestActorSystem.Instance);
                                    c.SerializerSettings.Converters.Add(new ObserverRefConverter(system));
                                    c.SerializerSettings.Converters.Add(new ClientRefConverter(system));
                                    c.SerializerSettings.Converters.Add(new ActorRefConverter(system));
                                    c.SerializerSettings.Converters.Add(new TypedActorRefConverter(system));
                                }));
                        });                    
                    })
                    .UseOrleankka()
                    .UseOrleankkaLegacyFeatures());

            var host = sb.Build();
            host.StartAsync().Wait();

            TestActorSystem.Host = host;
            TestActorSystem.Client = host.Services.GetRequiredService<IClusterClient>();
            TestActorSystem.Instance = host.ActorSystem();
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

            var timeout = TimeSpan.FromSeconds(5);

            TestActorSystem.Host.StopAsync().Wait(timeout);
            TestActorSystem.Host.Dispose();

            TestActorSystem.Client = null;
            TestActorSystem.Host = null;
            TestActorSystem.Instance = null;
        }
    }
}
