using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;

using Orleans;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.Runtime.Configuration;
using Orleans.Runtime.Host;

namespace Orleankka
{
    public partial class ActorSystem
    {
        public static ActorSystemConfiguration Configure()
        {
            return new ActorSystemConfiguration();
        }
    }

    public sealed class ActorSystemConfiguration
    {
        internal ActorSystemConfiguration()
        {}

        public ActorSystemClientConfiguration Client()
        {
            return new ActorSystemClientConfiguration();
        }

        public ActorSystemClusterConfiguration Cluster()
        {
            return new ActorSystemClusterConfiguration();
        }

        public ActorSystemEmbeddedConfiguration Embedded()
        {
            return new ActorSystemEmbeddedConfiguration();
        }
    }

    public sealed class ActorSystemClientConfiguration
    {
        internal ActorSystemClientConfiguration()
        {}
    }

    public sealed class ActorSystemClusterConfiguration
    {
        internal ActorSystemClusterConfiguration()
        {}
    }

    public sealed class ActorSystemEmbeddedConfiguration
    {
        ClusterConfiguration cluster;
        ClientConfiguration client;

        readonly HashSet<BootstrapperConfiguration> bootstrappers = new HashSet<BootstrapperConfiguration>();
        readonly Dictionary<string, Assembly> assemblies = new Dictionary<string, Assembly>();

        AppDomainSetup setup;

        internal ActorSystemEmbeddedConfiguration()
        {
            // can't get it to work programmatically
            cluster = new ClusterConfiguration()
                .LoadFromEmbeddedResource<Actor>("Configs.Default.Cluster.Configuration.xml");
            
            client = new ClientConfiguration
            {
                GatewayProvider = ClientConfiguration.GatewayProviderType.Config,
                ResponseTimeout = TimeSpan.FromSeconds(60),
                DefaultTraceLevel = Logger.Severity.Warning,
                TraceToConsole = true,
                TraceFileName = ""
            };

            client.Gateways.Add(new IPEndPoint(IPAddress.Loopback, 30000));
            client.TraceLevelOverrides.Add(new Tuple<string, Logger.Severity>("Application", Logger.Severity.Info));
        }

        public ActorSystemEmbeddedConfiguration InMemory()
        {
            cluster.Globals.LivenessType = GlobalConfiguration.LivenessProviderType.MembershipTableGrain;
            cluster.Globals.ReminderServiceType = GlobalConfiguration.ReminderServiceProviderType.ReminderTableGrain;

            return this;
        }

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

        public ActorSystemEmbeddedConfiguration Use<T>(Dictionary<string, string> properties = null) where T : Bootstrapper
        {
            if (!bootstrappers.Add(new BootstrapperConfiguration(typeof(T), properties)))
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

            var domain = AppDomain.CreateDomain("EmbeddedSilo", null, setup);
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
            ProviderCategoryConfiguration category = cluster.Globals.ProviderConfigurations.Find("Bootstrap");

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

        class EmbeddedActorSystem : IActorSystem
        {
            readonly IActorSystem system;
            readonly AppDomain domain;
            readonly SiloHost host;

            internal EmbeddedActorSystem(IActorSystem system, AppDomain domain, SiloHost host)
            {
                this.system = system;
                this.domain = domain;
                this.host = host;
            }

            public void Dispose()
            {
                host.StopOrleansSilo();
                host.Dispose();
                
                AppDomain.Unload(domain);
            }

            public ActorRef ActorOf(ActorPath path)
            {
                return system.ActorOf(path);
            }

            public ObserverRef ObserverOf(ObserverPath path)
            {
                return system.ObserverOf(path);
            }
        }

        class BootstrapperConfiguration : IEquatable<BootstrapperConfiguration>
        {
            readonly Type type;
            readonly Dictionary<string, string> properties;

            public BootstrapperConfiguration(Type type, Dictionary<string, string> properties)
            {
                this.type = type;
                this.properties = properties ?? new Dictionary<string, string>();
            }

            public bool Equals(BootstrapperConfiguration other)
            {
                return !ReferenceEquals(null, other) && (ReferenceEquals(this, other) || type == other.type);
            }

            public override bool Equals(object obj)
            {
                return !ReferenceEquals(null, obj) && (ReferenceEquals(this, obj) || Equals((BootstrapperConfiguration)obj));
            }

            public override int GetHashCode()
            {
                return type.GetHashCode();
            }

            public void Register(ProviderCategoryConfiguration category)
            {
                var config = new ProviderConfiguration(properties, type.FullName, "Boot" + type.FullName);

                var peskyField1 = typeof(ProviderConfiguration).GetField("childConfigurations", BindingFlags.Instance | BindingFlags.NonPublic);
                var peskyField2 = typeof(ProviderConfiguration).GetField("childProviders", BindingFlags.Instance | BindingFlags.NonPublic);

                Debug.Assert(peskyField1 != null);
                Debug.Assert(peskyField2 != null);

                peskyField1.SetValue(config, new List<ProviderConfiguration>());
                peskyField2.SetValue(config, new List<IProvider>());

                category.Providers.Add(config.Name, config);
            }
        }
    }
}