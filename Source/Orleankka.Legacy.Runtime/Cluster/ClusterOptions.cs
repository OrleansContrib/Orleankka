using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Orleans;
using Orleans.Hosting;
using Orleans.ApplicationParts;
using Orleans.Configuration;
using Orleans.Runtime;
using Orleans.Storage;
using Orleans.Streams;

namespace Orleankka.Legacy.Cluster
{
    using Behaviors;
    using Streams;
    using Utility;

    public class LegacyOrleankkaClusterOptions
    {
        readonly Dictionary<string, Action<OptionsBuilder<SimpleMessageStreamProviderOptions>>> smsStreamProviders = 
             new Dictionary<string, Action<OptionsBuilder<SimpleMessageStreamProviderOptions>>>();

        string[] persistentStreamProviders = new string[0];

        public LegacyOrleankkaClusterOptions AddSimpleMessageStreamProvider(string name, Action<OptionsBuilder<SimpleMessageStreamProviderOptions>> configureOptions = null)
        {
            Requires.NotNullOrWhitespace(name, nameof(name));
            smsStreamProviders.Add(name, configureOptions);
            return this;
        }

        public LegacyOrleankkaClusterOptions RegisterPersistentStreamProviders(params string[] names)
        {
            Requires.NotNull(names, nameof(names));
            persistentStreamProviders = names;
            return this;
        }

        internal void Configure(IApplicationPartManager apm, IServiceCollection services)
        {
            var assemblies = apm.ApplicationParts
                .OfType<AssemblyPart>().Select(x => x.Assembly)
                .Distinct()
                .ToArray();

            var actors = assemblies.SelectMany(x => x.GetTypes())
                .Where(x => typeof(Actor).IsAssignableFrom(x) && x.IsClass && !x.IsAbstract)
                .ToArray();

            RegisterStreamSubscriptions(services, actors);
            RegisterSimpleMessageStreamProviders(services);
            RegisterBehaviors(actors);

            BootStreamSubscriptions(services);
        }

        void RegisterSimpleMessageStreamProviders(IServiceCollection services)
        {
            foreach (var each in smsStreamProviders)
            {
                var name = each.Key;
                var configureOptions = each.Value;

                configureOptions?.Invoke(services.AddOptions<SimpleMessageStreamProviderOptions>(name));
                services.ConfigureNamedOptionForLogging<SimpleMessageStreamProviderOptions>(name);
                services.AddSingletonNamedService<IStreamProvider>(name, (s, n) => new SimpleMessageStreamProviderMatcher(s, n));
            }
        }

        static void RegisterStreamSubscriptions(IServiceCollection services, Type[] actors)
        {
            var subscriptions = new StreamSubscriptionSpecificationRegistry();
            
            foreach (var actor in actors)
            {
                var dispatcher = new Dispatcher(actor);
                subscriptions.Register(StreamSubscriptionSpecification.From(actor, dispatcher));
            }

            services.AddSingleton(subscriptions);
        }

        static void RegisterBehaviors(Type[] actors)
        {
            foreach (var each in actors)
                ActorBehavior.Register(each);
        }

        void BootStreamSubscriptions(IServiceCollection services)
        {
            const string name = "orlssb";
            services.AddOptions<PersistentStreamProviderMatcherOptions>(name).Configure(c => c.Providers = persistentStreamProviders);
            services.AddSingletonNamedService(name, PersistentStreamProviderMatcher.Create);
            services.AddSingletonNamedService(name, (s, n) => (ILifecycleParticipant<ISiloLifecycle>) s.GetRequiredServiceByName<IGrainStorage>(n));
        }
    }

    public static class LegacySiloHostBuilderExtensions
    {
        public static ISiloHostBuilder UseOrleankkaLegacyFeatures(this ISiloHostBuilder builder) => 
            UseOrleankkaLegacyFeatures(builder, new LegacyOrleankkaClusterOptions());

        public static ISiloHostBuilder UseOrleankkaLegacyFeatures(this ISiloHostBuilder builder, Func<LegacyOrleankkaClusterOptions, LegacyOrleankkaClusterOptions> configure) => 
            UseOrleankkaLegacyFeatures(builder, configure(new LegacyOrleankkaClusterOptions()));

        public static ISiloHostBuilder UseOrleankkaLegacyFeatures(this ISiloHostBuilder builder, LegacyOrleankkaClusterOptions cfg) => 
            builder.ConfigureServices(services => UseOrleankkaLegacyFeatures(builder.GetApplicationPartManager(), services, cfg));

        public static ISiloBuilder UseOrleankkaLegacyFeatures(this ISiloBuilder builder) => 
            UseOrleankkaLegacyFeatures(builder, new LegacyOrleankkaClusterOptions());

        public static ISiloBuilder UseOrleankkaLegacyFeatures(this ISiloBuilder builder, Func<LegacyOrleankkaClusterOptions, LegacyOrleankkaClusterOptions> configure) => 
            UseOrleankkaLegacyFeatures(builder, configure(new LegacyOrleankkaClusterOptions()));

        public static ISiloBuilder UseOrleankkaLegacyFeatures(this ISiloBuilder builder, LegacyOrleankkaClusterOptions cfg) => 
            builder.ConfigureServices(services => UseOrleankkaLegacyFeatures(builder.GetApplicationPartManager(), services, cfg));

        static void UseOrleankkaLegacyFeatures(IApplicationPartManager apm, IServiceCollection services, LegacyOrleankkaClusterOptions cfg) => 
            cfg.Configure(apm, services);
    }
}