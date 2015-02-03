using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;

using Orleans;
using Orleans.Providers;
using Orleans.Runtime.Configuration;
using Orleans.Runtime.Host;

namespace Orleankka.Configuration.Embedded
{
    public class ActorSystemEmbeddedConfiguration
    {
        internal ClusterConfiguration cluster;
        internal ClientConfiguration client;

        readonly HashSet<BootstrapProviderConfiguration> bootstrappers = new HashSet<BootstrapProviderConfiguration>();
        readonly Dictionary<string, Assembly> assemblies = new Dictionary<string, Assembly>();

        AppDomainSetup setup;

        public ActorSystemEmbeddedConfiguration With(ClusterConfiguration config)
        {
            Requires.NotNull(config, "config");
            cluster = config;
            return this;
        }

        public ActorSystemEmbeddedConfiguration With(ClientConfiguration config)
        {
            Requires.NotNull(config, "config");
            client = config;
            return this;
        }

        public ActorSystemEmbeddedConfiguration Use<T>(Dictionary<string, string> properties = null) where T : ActorSystemBootstrapper
        {
            if (!bootstrappers.Add(new BootstrapProviderConfiguration(typeof(T), properties)))
                throw new ArgumentException(
                    string.Format("Bootstrapper of the type {0} has been already registered", typeof(T)));

            return this;
        }

        public ActorSystemEmbeddedConfiguration With(AppDomainSetup setup)
        {
            Requires.NotNull(setup, "setup");
            this.setup = setup;
            return this;
        }

        public ActorSystemEmbeddedConfiguration Register(params Assembly[] assemblies)
        {
            Requires.NotNull(assemblies, "assemblies");

            foreach (var assembly in assemblies)
            {
                if (this.assemblies.ContainsKey(assembly.FullName))
                    throw new ArgumentException(
                        string.Format("Assembly {0} has been already registered", assembly.FullName));

                this.assemblies.Add(assembly.FullName, assembly);
            }

            return this;
        }

        public IActorSystem Done()
        {
            if (client == null || cluster == null)
                throw new InvalidOperationException("Both client and server configs should be provided before starting an embedded silo");

            if (setup == null)
                setup = AppDomain.CurrentDomain.SetupInformation;

            RegisterBootstrappers();

            var hostType = typeof(SiloHost);
            var hostConstructorArgs = new object[] {Dns.GetHostName(), cluster};

            var domain = AppDomain.CreateDomain("Playground", null, setup);
            var host = (SiloHost)domain.CreateInstanceAndUnwrap(
                        hostType.Assembly.FullName, hostType.FullName, false,
                        BindingFlags.Public | BindingFlags.Instance, null,
                        hostConstructorArgs, null, null);

            RegisterClientAssemblies();
            RegisterServerAssemblies(domain);

            host.LoadOrleansConfig();
            host.InitializeOrleansSilo();
            host.StartOrleansSilo();

            GrainClient.Initialize(client);

            return new EmbeddedActorSystem(ActorSystem.Instance, domain, host);
        }
        
        void RegisterBootstrappers()
        {
            var category = cluster.Globals.ProviderConfigurations.Find("Bootstrap");

            if (category == null)
            {
                category = new ProviderCategoryConfiguration
                {
                    Name = "Bootstrap",
                    Providers = new Dictionary<string, IProviderConfiguration>()
                };

                cluster.Globals.ProviderConfigurations.Add("Bootstrap", category);
            }

            foreach (var bootstrapper in bootstrappers)
                bootstrapper.Register(category);
        }

        void RegisterClientAssemblies()
        {
            foreach (var assembly in assemblies.Values)
                ActorSystem.Register(assembly);
        }

        void RegisterServerAssemblies(AppDomain domain)
        {
            var type = typeof(AssemblyRegistrar);

            var registrar = (AssemblyRegistrar)domain.CreateInstanceAndUnwrap(
                            type.Assembly.FullName, type.FullName, false,
                            BindingFlags.Public | BindingFlags.Instance, null,
                            new object[0], null, null);

            registrar.Register(assemblies.Values);
        }

        class AssemblyRegistrar : MarshalByRefObject
        {
            public void Register(IEnumerable<Assembly> assemblies)
            {
                foreach (var assembly in assemblies)
                    ActorSystem.Register(assembly);
            }
        }
    }
}