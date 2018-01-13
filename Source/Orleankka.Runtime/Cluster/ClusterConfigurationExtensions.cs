using System;
using System.Reflection;

using Orleans.Runtime.Configuration;

namespace Orleankka.Cluster
{
    using Utility;

    public static class ClusterConfigurationExtensions
    {
        public static ClusterConfiguration LoadFromEmbeddedResource<TNamespaceScope>(this ClusterConfiguration config, string resourceName)
        {
            return LoadFromEmbeddedResource(config, typeof(TNamespaceScope), resourceName);
        }

        public static ClusterConfiguration LoadFromEmbeddedResource(this ClusterConfiguration config, Type namespaceScope, string resourceName)
        {
            if (namespaceScope.Namespace == null)
                throw new ArgumentException("Resource assembly and scope cannot be determined from type '0' since it has no namespace.\nUse overload that takes Assembly and string path to provide full path of the embedded resource");

            return LoadFromEmbeddedResource(config, namespaceScope.Assembly, $"{namespaceScope.Namespace}.{resourceName}");
        }

        public static ClusterConfiguration LoadFromEmbeddedResource(this ClusterConfiguration config, Assembly assembly, string fullResourcePath)
        {
            var result = new ClusterConfiguration();
            result.Load(assembly.LoadEmbeddedResource(fullResourcePath));
            return result;
        }

        public static ClusterConfiguration DefaultKeepAliveTimeout(this ClusterConfiguration config, TimeSpan idle)
        {
            Requires.NotNull(config, nameof(config));
            config.Globals.Application.SetDefaultCollectionAgeLimit(idle);
            return config;
        }
    }
}