using System;
using System.Diagnostics.CodeAnalysis;

using NUnit.Framework;
using NUnit.Framework.Interfaces;

using Orleankka.Core;
using Orleankka.Features.Storage_provider_facet;
using Orleankka.Testing;

using Orleans.Configuration;
using Orleans.Runtime;
using Orleans.Storage;

[assembly: TeardownSilo]

namespace Orleankka.Testing
{
    using Playground;
    using Utility;
    using Features.Intercepting_requests;

    using Orleans.Hosting;
    using Orleans.Providers.Streams.AzureQueue;

    [AttributeUsage(AttributeTargets.Class)]
    public class RequiresSiloAttribute : TestActionAttribute
    {
        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
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
                        
                        x.ActorMiddleware(typeof(TestActorBase), new TestActorMiddleware());
                        x.ActorRefMiddleware(new TestActorRefMiddleware());

                        x.Services(services => services
                            .AddSingletonNamedService<IGrainStorage>("test", (sp, name) => new TestStorageProvider(name)));

                        x.Builder(b =>
                        {
                            b.UseAzureTableReminderService("UseDevelopmentStorage=true");
                            b.AddAzureQueueStreams<AzureQueueDataAdapterV2>("aqp", options =>
                            {
                                options.Configure(c => c.ConnectionString = "UseDevelopmentStorage=true");
                            });

                            b.AddStartupTask(Features.Autorun_actors.StartupTask.Run);
                            
                            b.Configure<GrainCollectionOptions>(o =>
                            {
                                o.CollectionAge = TimeSpan.FromMinutes(1);
                                o.CollectionQuantum = TimeSpan.FromSeconds(10);

                                o.ClassSpecificCollectionAge[$"Fun.{typeof(Features.Keep_alive.ITestActor).FullName}"] = TimeSpan.FromMinutes(2);
                            });
                        });

                        x.RegisterPersistentStreamProviders("aqp");
                    })
                    .Client(x =>
                    {
                        x.ActorRefMiddleware(new TestActorRefMiddleware());

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