using System;
using System.Collections.Generic;

using Orleans.Runtime.Configuration;

namespace Orleankka.Core
{
    class StreamProviderConfiguration
    {
        readonly string name;
        readonly Type type;
        readonly IDictionary<string, string> properties;

        public StreamProviderConfiguration(string name, Type type, IDictionary<string, string> properties)
        {
            this.name = name;
            this.type = type;
            this.properties = properties ?? new Dictionary<string, string>();
        }

        public void Register(ClientConfiguration configuration)
        {
            properties.Add(StreamProvider.TypeKey, type.AssemblyQualifiedName);
            configuration.RegisterStreamProvider(typeof(StreamProvider).FullName, name, properties);
        }

        public void Register(ClusterConfiguration configuration)
        {
            properties.Add(StreamProvider.TypeKey, type.AssemblyQualifiedName);
            configuration.Globals.RegisterStreamProvider(typeof(StreamProvider).FullName, name, properties);
        }
    }
}