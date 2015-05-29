using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

using Orleans.Providers;
using Orleans.Runtime.Configuration;

namespace Orleankka.Cluster
{
    public interface IBootstrapper
    {
        /// <summary>
        /// Runs the bootstrapper passing the properties specified during actor system configuration.
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <returns>The promise</returns>
        Task Run(object properties);
    }

    public abstract class Bootstrapper<TProperties> : IBootstrapper
    {
        Task IBootstrapper.Run(object properties)
        {
            return Run((TProperties) properties);
        }

        /// <summary>
        /// Runs the bootstrapper passing the properties specified during actor system configuration.
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <returns>The promise</returns>
        public abstract Task Run(TProperties properties);
    }

    class BootstrapProvider : IBootstrapProvider
    {
        internal const string TypeKey = "<-::Type::->";
        internal const string PropertiesKey = "<-::Properties::->";

        public string Name
        {
            get; private set;
        }

        Task IProvider.Init(string name, IProviderRuntime providerRuntime, IProviderConfiguration config)
        {
            Name = name;

            var type = Type.GetType(config.Properties[TypeKey]);
            Debug.Assert(type != null);

            var bootstrapper = (IBootstrapper)Activator.CreateInstance(type);
            return bootstrapper.Run(Deserialize(config.Properties[PropertiesKey]));
        }

        static object Deserialize(string s)
        {
            if (s == null)
                return null;

            var bytes = Convert.FromBase64String(s);

            using (var ms = new MemoryStream(bytes))
            {
                var formatter = new BinaryFormatter();
                return formatter.Deserialize(ms);
            }
        }
    }

    class BootstrapProviderConfiguration : IEquatable<BootstrapProviderConfiguration>
    {
        readonly Type type;
        readonly Dictionary<string, string> properties = new Dictionary<string, string>();

        public BootstrapProviderConfiguration(Type type, object properties)
        {
            this.type = type;
            this.properties.Add(BootstrapProvider.TypeKey, type.AssemblyQualifiedName);
            this.properties.Add(BootstrapProvider.PropertiesKey, Serialize(properties));
        }

        static string Serialize(object obj)
        {
            if (obj == null)
                return null;

            using (var ms = new MemoryStream())
            {
                new BinaryFormatter().Serialize(ms, obj);
                return Convert.ToBase64String(ms.ToArray());
            }
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