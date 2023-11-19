using System;
using System.Linq;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Orleans.Serialization;

namespace Orleankka.Legacy.Cluster
{
    using Behaviors;

    using Streams;
    using Utility;

    using Orleans;
    using Orleans.Runtime;

    public class LegacyOrleankkaClusterOptions
    {
        string[] persistentStreamProviders = new string[0];

        public LegacyOrleankkaClusterOptions RegisterPersistentStreamProviders(params string[] names)
        {
            Requires.NotNull(names, nameof(names));
            persistentStreamProviders = names;
            return this;
        }

        internal void Configure(IServiceCollection services)
        {
            var assemblies = services.GetRelevantAssemblies()
                .Distinct()
                .ToArray();

            var actors = assemblies.SelectMany(x => x.GetTypes())
                .Where(x => typeof(Actor).IsAssignableFrom(x) && x.IsClass && !x.IsAbstract)
                .ToArray();

            RegisterBehaviors(actors);
            BootStreamSubscriptions(services);
        }

        static void RegisterBehaviors(Type[] actors)
        {
            foreach (var each in actors)
                ActorBehavior.Register(each);
        }

        void BootStreamSubscriptions(IServiceCollection services)
        {
            services.AddSingleton(sp => ActivatorUtilities.CreateInstance<StreamSubscriptionTable>(sp));

            foreach (var provider in persistentStreamProviders)
            {
                services.AddSingleton<ILifecycleParticipant<ISiloLifecycle>>(sp => new StreamProviderPubSubRegistrar(sp, provider));
            }
        }
    }

    public static class LegacySiloHostBuilderExtensions
    {
        public static IHostBuilder UseOrleankkaLegacyFeatures(this IHostBuilder builder) => 
            UseOrleankkaLegacyFeatures(builder, new LegacyOrleankkaClusterOptions());

        public static IHostBuilder UseOrleankkaLegacyFeatures(this IHostBuilder builder, Func<LegacyOrleankkaClusterOptions, LegacyOrleankkaClusterOptions> configure) => 
            UseOrleankkaLegacyFeatures(builder, configure(new LegacyOrleankkaClusterOptions()));
        
        public static IHostBuilder UseOrleankkaLegacyFeatures(this IHostBuilder builder, LegacyOrleankkaClusterOptions cfg) => 
            builder.ConfigureServices((_, services) => UseOrleankkaLegacyFeatures(services, cfg));

        static void UseOrleankkaLegacyFeatures(IServiceCollection services, LegacyOrleankkaClusterOptions cfg) => 
            cfg.Configure(services);
    }
}