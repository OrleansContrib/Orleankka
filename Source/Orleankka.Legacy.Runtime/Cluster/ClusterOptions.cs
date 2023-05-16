using System;
using System.Linq;

using Microsoft.Extensions.DependencyInjection;

using Orleans.Hosting;

namespace Orleankka.Legacy.Cluster
{
    using Behaviors;

    public class LegacyOrleankkaClusterOptions
    {
        internal void Configure(IServiceCollection services)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
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
        public static ISiloBuilder UseOrleankkaLegacyFeatures(this ISiloBuilder builder) => 
            UseOrleankkaLegacyFeatures(builder, new LegacyOrleankkaClusterOptions());

        public static ISiloBuilder UseOrleankkaLegacyFeatures(this ISiloBuilder builder, Func<LegacyOrleankkaClusterOptions, LegacyOrleankkaClusterOptions> configure) => 
            UseOrleankkaLegacyFeatures(builder, configure(new LegacyOrleankkaClusterOptions()));
        
        public static ISiloBuilder UseOrleankkaLegacyFeatures(this ISiloBuilder builder, LegacyOrleankkaClusterOptions cfg) => 
            builder.ConfigureServices(services => UseOrleankkaLegacyFeatures(services, cfg));

        static void UseOrleankkaLegacyFeatures(IServiceCollection services, LegacyOrleankkaClusterOptions cfg) => 
            cfg.Configure(services);
    }
}