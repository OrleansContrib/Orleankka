using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Resources;

using Orleans;
using Orleans.Host;
using Orleans.Providers;
using Orleans.Runtime.Configuration;

namespace Orleankka
{
    public class EmbeddedSilo
    {
        ServerConfiguration server;
        ClientConfiguration client;

        Type bootstrapperType;
        Dictionary<string, string> bootstrapperProperties;

        AppDomainSetup setup;

        public EmbeddedSilo With(ServerConfiguration config)
        {
            Requires.NotNull(config, "config");
            server = config;
            return this;
        }

        public EmbeddedSilo With(ClientConfiguration config)
        {
            Requires.NotNull(config, "config");
            client = config;
            return this;
        }

        public EmbeddedSilo Use<TBootstrapper>(Dictionary<string, string> properties = null) where TBootstrapper : Bootstrapper
        {
            bootstrapperType = typeof(TBootstrapper);
            bootstrapperProperties = properties;
            return this;
        }

        public EmbeddedSilo With(AppDomainSetup setup)
        {
            Requires.NotNull(setup, "setup");
            this.setup = setup;
            return this;
        }

        public IDisposable Start()
        {
            if (client == null || server == null)
                throw new InvalidOperationException("Both client and server configs should be provided before starting an embedded silo");

            if (setup == null)
                setup = AppDomain.CurrentDomain.SetupInformation;

            var hostType = typeof(OrleansSiloHost);
            var hostConstructorArgs = new object[]{Dns.GetHostName()};

            var domain = AppDomain.CreateDomain("EmbeddedSilo", null, setup);
            var host = (OrleansSiloHost) domain.CreateInstanceAndUnwrap(
                hostType.Assembly.FullName, hostType.FullName, false, 
                BindingFlags.Public | BindingFlags.Instance, null, 
                hostConstructorArgs, null, null);

            if (bootstrapperType != null)
            {
                var bootstrapper = new ProviderConfiguration(
                    bootstrapperProperties ?? new Dictionary<string, string>(), 
                    bootstrapperType.FullName, "Boot"
                );

                var peskyField1 = typeof(ProviderConfiguration).GetField("_childConfigurations", BindingFlags.Instance | BindingFlags.NonPublic);
                var peskyField2 = typeof(ProviderConfiguration).GetField("_childProviders", BindingFlags.Instance | BindingFlags.NonPublic);

                Debug.Assert(peskyField1 != null);
                Debug.Assert(peskyField2 != null);

                peskyField1.SetValue(bootstrapper, new List<ProviderConfiguration>());
                peskyField2.SetValue(bootstrapper, new List<IOrleansProvider>());

                server.Globals.ProviderConfigurations.Add("Bootstrap", new ProviderCategoryConfiguration
                {
                    Name = "Bootstrap",
                    Providers = new Dictionary<string, IProviderConfiguration> {{bootstrapper.Name, bootstrapper}}
                });
            }

            host.SetOrleansConfig(server);
            host.LoadOrleansConfig();

            host.InitializeOrleansSilo();
            host.StartOrleansSilo();

            OrleansClient.Initialize(client);

            return new Disposable(domain, host);
        }

        class Disposable : IDisposable
        {
            readonly AppDomain domain;
            readonly OrleansSiloHost host;

            internal Disposable(AppDomain domain, OrleansSiloHost host)
            {
                this.domain = domain;
                this.host = host;
            }

            public void Dispose()
            {
                host.StopOrleansSilo();
                host.Dispose();

                AppDomain.Unload(domain);
            }
        }
    }

    /// <summary>
    /// Data object holding server (silo) configuration parameters.
    /// </summary>
    [Serializable]
    public class ServerConfiguration : OrleansConfiguration
    {
        // just an alias for consistency (Client/Server)
    }

    public static class EmbeddedSiloConfigurationExtensions
    {
        public static ClientConfiguration LoadFromEmbeddedResource<TNamespaceScope>(this ClientConfiguration config, string resourceName)
        {
            return LoadFromEmbeddedResource(config, typeof(TNamespaceScope), resourceName);
        }

        public static ClientConfiguration LoadFromEmbeddedResource(this ClientConfiguration config, Type namespaceScope, string resourceName)
        {
            return LoadFromEmbeddedResource(config, namespaceScope.Assembly, string.Format("{0}.{1}", namespaceScope.Namespace, resourceName));
        }

        public static ClientConfiguration LoadFromEmbeddedResource(this ClientConfiguration config, Assembly assembly, string fullResourcePath)
        {
            var result = new ClientConfiguration();

            var loader = result.GetType().GetMethod("Load", BindingFlags.Instance | BindingFlags.NonPublic, null, new[] {typeof(TextReader)}, null);
            loader.Invoke(result, new object[]{LoadFromEmbeddedResource(assembly, fullResourcePath)});

            return result;
        }

        public static ServerConfiguration LoadFromEmbeddedResource<TNamespaceScope>(this ServerConfiguration config, string resourceName)
        {
            return LoadFromEmbeddedResource(config, typeof(TNamespaceScope), resourceName);
        }

        public static ServerConfiguration LoadFromEmbeddedResource(this ServerConfiguration config, Type namespaceScope, string resourceName)
        {
            return LoadFromEmbeddedResource(config, namespaceScope.Assembly, string.Format("{0}.{1}", namespaceScope.Namespace, resourceName));
        }

        public static ServerConfiguration LoadFromEmbeddedResource(this ServerConfiguration config, Assembly assembly, string fullResourcePath)
        {
            var result = new ServerConfiguration();
            result.Load(LoadFromEmbeddedResource(assembly, fullResourcePath));
            return result;
        }

        static TextReader LoadFromEmbeddedResource(Assembly assembly, string fullResourcePath)
        {
            using (var stream = assembly.GetManifestResourceStream(fullResourcePath))
            {
                if (stream == null)
                    throw new MissingManifestResourceException(
                        string.Format("Unable to find resource with the path {0} in assembly {1}", fullResourcePath, assembly.FullName));

                return new StringReader(new StreamReader(stream).ReadToEnd());
            }
        }
    }
}
