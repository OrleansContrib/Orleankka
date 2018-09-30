using System;
using System.Linq;
using System.Net;
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using Orleans;
using Orleans.Configuration;
using Orleans.Runtime;
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

            var builder = new ClientBuilder()
                .Configure<ClusterOptions>(x =>
                {
                    x.ClusterId = "test";
                    x.ServiceId = "test-service";
                })
                .UseStaticClustering(x => x.Gateways.Add(new IPEndPoint(0, 0).ToGatewayUri()))
                .ConfigureApplicationParts(apm => apm.AddFromAppDomain());

            if (serializers.Length > 0)
            {
                builder.ConfigureServices(services => services.Configure<SerializationProviderOptions>(options =>
                {
                    options.SerializationProviders.AddRange(serializers.Select(x => x.GetTypeInfo()));
                }));
            }

            var client = builder.Build();
            Manager = client.ServiceProvider.GetRequiredService<SerializationManager>();
        }

        public object Roundtrip(object obj) => Manager != null
            ? Manager.RoundTripSerializationForTesting(obj)
            : obj;
    }
}