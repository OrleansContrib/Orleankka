using System;
using System.Collections.Generic;

using Orleans.Runtime.Configuration;

namespace Orleankka.Client
{
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

        public void Register(ClientConfiguration configuration)
        {
            configuration.RegisterStreamProvider(type.FullName, Name, properties);
        }
    }
}