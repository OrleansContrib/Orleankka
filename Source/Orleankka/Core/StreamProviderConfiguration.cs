using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Orleans.Providers.Streams.Common;
using Orleans.Runtime.Configuration;

namespace Orleankka.Core
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

        public void Register(ClusterConfiguration configuration)
        {
            configuration.Globals.RegisterStreamProvider(type.FullName, Name, properties);
        }

        public bool IsPersistentStreamProvider()
        {
            Debug.Assert(type.BaseType != null);
            return type.BaseType.IsConstructedGenericType &&
                   type.BaseType.GetGenericTypeDefinition() == typeof(PersistentStreamProvider<>);
        }
    }
}