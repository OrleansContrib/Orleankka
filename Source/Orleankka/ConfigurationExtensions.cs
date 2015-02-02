using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;

using Orleans.Runtime.Configuration;

namespace Orleankka
{
    public static class ConfigurationExtensions
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

        public static ClusterConfiguration LoadFromEmbeddedResource<TNamespaceScope>(this ClusterConfiguration config, string resourceName)
        {
            return LoadFromEmbeddedResource(config, typeof(TNamespaceScope), resourceName);
        }

        public static ClusterConfiguration LoadFromEmbeddedResource(this ClusterConfiguration config, Type namespaceScope, string resourceName)
        {
            return LoadFromEmbeddedResource(config, namespaceScope.Assembly, string.Format("{0}.{1}", namespaceScope.Namespace, resourceName));
        }

        public static ClusterConfiguration LoadFromEmbeddedResource(this ClusterConfiguration config, Assembly assembly, string fullResourcePath)
        {
            var result = new ClusterConfiguration();
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
