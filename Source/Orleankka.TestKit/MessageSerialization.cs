using System;
using System.Linq;
using System.Net;
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using Orleans;
using Orleans.Runtime.Configuration;
using Orleans.Serialization;

namespace Orleankka.TestKit
{
    using Utility;

    public class MessageSerialization
    {
        internal static readonly MessageSerialization Default = new MessageSerialization(false);

        public readonly SerializationManager Manager;

        public MessageSerialization(bool roundtrip, params Type[] serializers)
        {
            Requires.NotNull(serializers, nameof(serializers));

            if (!roundtrip)
                return;

            var configuration = new ClientConfiguration
            {
                ClusterId = "MessageSerialization",
                GatewayProvider = ClientConfiguration.GatewayProviderType.Config,
                Gateways = { new IPEndPoint(0, 0) },
            };

            if (serializers.Length > 0)
                configuration.SerializationProviders.AddRange(serializers.Select(x => x.GetTypeInfo()));

            var client = new ClientBuilder()
                .UseConfiguration(configuration)
                .ConfigureApplicationParts(apm => apm.AddFromAppDomain())
                .Build();

            Manager = client.ServiceProvider.GetRequiredService<SerializationManager>();
        }

        public object Roundtrip(object obj) => Manager != null
            ? Manager.RoundTripSerializationForTesting(obj)
            : obj;
    }
}