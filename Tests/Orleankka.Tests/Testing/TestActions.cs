﻿using System;
using System.Linq;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;
using NUnit.Framework.Interfaces;

using Orleans;
using Orleans.Hosting;
using Orleans.Configuration;
using Orleans.Storage;
using Orleans.Runtime;
using Orleans.Serialization;

using Orleankka.Testing;
using Orleankka.Cluster;
using Orleankka.Features.Intercepting_requests;
using Orleankka.Legacy.Cluster;

[assembly: TeardownSilo]

namespace Orleankka.Testing
{
    using Features.State_persistence;

    [AttributeUsage(AttributeTargets.Class)]
    public class RequiresSiloAttribute : TestActionAttribute
    {
        public override void BeforeTest(ITest test)
        {
            if (!test.IsSuite)
                return;

            if (TestActorSystem.Instance != null)
                return;

            var sb = new HostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddKeyedSingleton<IGrainStorage>("test", (sp, name) => new TestStorageProvider((string)name));
                    services.Configure<GrainCollectionOptions>(options => options.CollectionAge = TimeSpan.FromMinutes(1.1));
                    
                    services.Configure<DispatcherOptions>(o =>
                    {
                        var customConventions = Dispatcher.DefaultHandlerNamingConventions.Concat(new[]{"OnFoo"});
                        o.HandlerNamingConventions = customConventions.ToArray();
                    });

                    services.AddSingleton<IActorRefMiddleware>(s => new TestActorRefMiddleware());
                    services.AddSingleton<IActorMiddleware>(s => new TestActorMiddleware());
                    services.AddSingleton<IStreamRefMiddleware>(s => new TestStreamRefMiddleware());

                    services.AddSerializer(serializerBuilder =>
                    {
                        serializerBuilder.AddNewtonsoftJsonSerializer(
                            isSupported: type => typeof(Meta.Message).IsAssignableFrom(type), 
                            options => options.Configure(c =>
                            {
                                var system = new Lazy<IActorSystem>(()=> TestActorSystem.Instance);
                                var services = new Lazy<IServiceProvider>(()=> TestActorSystem.Client.ServiceProvider);
                                c.SerializerSettings = new Newtonsoft.Json.JsonSerializerSettings();
                                c.SerializerSettings.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto;
                                c.SerializerSettings.Converters.Add(new ObserverRefConverter(system));
                                c.SerializerSettings.Converters.Add(new ClientRefConverter(system));
                                c.SerializerSettings.Converters.Add(new ActorRefConverter(system));
                                c.SerializerSettings.Converters.Add(new TypedActorRefConverter(system));
                                c.SerializerSettings.Converters.Add(new StreamRefConverter(services));
                            }));
                    });                    
                })
                .UseOrleans((_, builder) => builder
                    .UseLocalhostClustering()
                    .AddMemoryGrainStorageAsDefault()
                    .AddMemoryGrainStorage("PubSubStore")
                    .AddMemoryStreams("sms")
                    .UseInMemoryReminderService())
                .UseOrleankka()
                .UseOrleankkaLegacyFeatures();

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
