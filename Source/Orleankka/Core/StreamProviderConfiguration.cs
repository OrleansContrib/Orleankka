using System;
using System.Collections.Generic;

using Orleans.Internals;
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
            if (IsPersistentStreamProvider())
            {
                configuration.RegisterStreamProvider(type.FullName, Name, properties);
                return;
            }

            properties.Add(StreamSubscriptionMatcher.TypeKey, type.AssemblyQualifiedName);
            configuration.RegisterStreamProvider(typeof(StreamSubscriptionMatcher).FullName, Name, properties);
        }

        public void Register(ClusterConfiguration configuration)
        {
            if (IsPersistentStreamProvider())
            {
                configuration.Globals.RegisterStreamProvider(type.FullName, Name, properties);
                return;
            }

            properties.Add(StreamSubscriptionMatcher.TypeKey, type.AssemblyQualifiedName);
            configuration.Globals.RegisterStreamProvider(typeof(StreamSubscriptionMatcher).FullName, Name, properties);
        }

        public bool IsPersistentStreamProvider() => type.IsPersistentStreamProvider();
    }
}