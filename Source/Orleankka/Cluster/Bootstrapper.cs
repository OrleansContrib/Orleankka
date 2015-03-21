using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Orleans;
using Orleans.Providers;
using Orleans.Runtime.Configuration;

namespace Orleankka.Cluster
{
    public abstract class Bootstrapper
    {
        /// <summary>
        /// Runs the bootstrapper passing the properties specified during actor system configuration.
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <returns>The promise</returns>
        public virtual Task Run(IDictionary<string, string> properties)
        {
            return TaskDone.Done;
        }
    }

    class BootstrapProvider : IBootstrapProvider
    {
        internal const string TypeKey = "<-::Type::->";

        public string Name
        {
            get;
            private set;
        }

        Task IProvider.Init(string name, IProviderRuntime providerRuntime, IProviderConfiguration config)
        {
            Name = name;

            var type = Type.GetType(config.Properties[TypeKey]);
            Debug.Assert(type != null);

            var bootstrapper = (Bootstrapper)Activator.CreateInstance(type);
            return bootstrapper.Run(config.Properties);
        }
    }

    class BootstrapProviderConfiguration : IEquatable<BootstrapProviderConfiguration>
    {
        readonly Type type;
        readonly Dictionary<string, string> properties;

        public BootstrapProviderConfiguration(Type type, Dictionary<string, string> properties)
        {
            this.type = type;
            this.properties = properties ?? new Dictionary<string, string>();
            this.properties.Add(BootstrapProvider.TypeKey, type.AssemblyQualifiedName);
        }

        public bool Equals(BootstrapProviderConfiguration other)
        {
            return !ReferenceEquals(null, other) && (ReferenceEquals(this, other) || type == other.type);
        }

        public override bool Equals(object obj)
        {
            return !ReferenceEquals(null, obj) && (ReferenceEquals(this, obj) || Equals((BootstrapProviderConfiguration)obj));
        }

        public override int GetHashCode()
        {
            return type.GetHashCode();
        }

        public void Register(GlobalConfiguration category)
        {
            category.RegisterBootstrapProvider<BootstrapProvider>(type.FullName, properties);
        }
    }
}