using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

using Orleans;
using Orleans.Providers;
using Orleans.Runtime.Configuration;

namespace Orleankka
{
    public abstract class ActorSystemBootstrapper
    {
        /// <summary>
        /// The serialization function, which serializes messages to byte[]
        /// </summary>
        /// <remarks>
        /// By default uses standard binary serialization provided by <see cref="BinaryFormatter"/>
        /// </remarks>
        public virtual Func<object, byte[]> Serializer
        {
            get { return null; }
        }

        public virtual Func<byte[], object> Deserializer
        {
            get { return null; }
        }
        
        /// <summary>
        /// The activation function, which creates actual instances of <see cref="Actor"/>
        /// </summary>
        public virtual Func<Type, Actor> Activator
        {
            get { return null; }
        }

        /// <summary>
        /// Runs the bootsrapper passing specified properties.
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <returns>The promise</returns>
        public virtual Task Run(IDictionary<string, string> properties)
        {
            return TaskDone.Done;
        }
    }

    class BootstrapProvider: IBootstrapProvider
    {
        internal const string TypeKey = "<-::Type::->";

        public string Name
        {
            get; private set;
        }

        Task IProvider.Init(string name, IProviderRuntime providerRuntime, IProviderConfiguration config)
        {
            Name = name;

            var type = Type.GetType(config.Properties[TypeKey]);
            Debug.Assert(type != null);

            var bootstrapper = (ActorSystemBootstrapper) Activator.CreateInstance(type);
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

        public void Register(ProviderCategoryConfiguration category)
        {
            var fullName = typeof(BootstrapProvider).FullName;
            var config = new ProviderConfiguration(properties, fullName, fullName);

            var peskyField1 = GetPrivateField("childConfigurations");
            var peskyField2 = GetPrivateField("childProviders");

            peskyField1.SetValue(config, new List<ProviderConfiguration>());
            peskyField2.SetValue(config, new List<IProvider>());

            category.Providers.Add(config.Name, config);
        }

        static FieldInfo GetPrivateField(string name)
        {
            var field = typeof(ProviderConfiguration)
                .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);

            Debug.Assert(field != null);
            return field;
        }
    }
}