using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

using Orleans.Streams;
using Orleans.Providers;
using Orleans.Runtime.Configuration;

namespace Orleankka.Core
{
    public class StreamProvider : IStreamProviderImpl
    {
        internal const string TypeKey = "<-::Type::->";
        IStreamProviderImpl provider;

        public Task Init(string name, IProviderRuntime providerRuntime, IProviderConfiguration config)
        {
            Name = name;

            var type = Type.GetType(config.Properties[TypeKey]);
            config.RemoveProperty(TypeKey);
            Debug.Assert(type != null);

            provider = (IStreamProviderImpl)Activator.CreateInstance(type);
            return provider.Init(name, providerRuntime, config);
        }

        public string Name { get; private set; }
        public bool IsRewindable => provider.IsRewindable;

        public Task Start() => provider.Start();
        public Task Stop() => provider.Stop();

        public IAsyncStream<T> GetStream<T>(Guid streamId, string streamNamespace)
        {
            return provider.GetStream<T>(streamId, streamNamespace);
        }
    }

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
            configuration.RegisterStreamProvider(type.FullName, name, properties);
        }

        public void Register(ClusterConfiguration configuration)
        {
            properties.Add(StreamProvider.TypeKey, type.AssemblyQualifiedName);
            configuration.Globals.RegisterStreamProvider(typeof(StreamProvider).FullName, name, properties);
        }
    }
}