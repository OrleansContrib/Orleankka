using System;
using System.Collections.Generic;

using Orleans.Runtime.Configuration;

namespace Orleankka.Cluster
{
    using Core.Streams;

    class StreamProviderConfiguration
    {
        readonly Type type;
        readonly IDictionary<string, string> properties;

        public StreamProviderConfiguration(string name, Type type, IDictionary<string, string> properties)
        {
            this.Name = name;
            this.type = type;
            this.properties = properties ?? new Dictionary<string, string>();
        }

        public string Name { get; }

        public void Register(ClusterConfiguration configuration)
        {
            properties.Add(StreamSubscriptionMatcher.TypeKey, type.AssemblyQualifiedName);
            configuration.Globals.RegisterStreamProvider(typeof(StreamSubscriptionMatcher).FullName, Name, properties);
        }
    }
}