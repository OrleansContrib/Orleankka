using System;
using System.Linq;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Orleans.Serialization;

namespace Orleankka.Legacy.Cluster
{
    using Behaviors;

    public class LegacyOrleankkaClusterOptions
    {
        internal void Configure(IServiceCollection services)
        {
            var assemblies = services.GetRelevantAssemblies()
                .Distinct()
                .ToArray();

            var actors = assemblies.SelectMany(x => x.GetTypes())
                .Where(x => typeof(Actor).IsAssignableFrom(x) && x.IsClass && !x.IsAbstract)
                .ToArray();

            RegisterBehaviors(actors);
        }

        static void RegisterBehaviors(Type[] actors)
        {
            foreach (var each in actors)
                ActorBehavior.Register(each);
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