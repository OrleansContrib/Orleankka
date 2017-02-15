using System;
using System.Linq;
using System.Reflection;

using Orleans.Serialization;

namespace Orleankka.TestKit
{
    public class SerializationOptions
    {
        internal static readonly SerializationOptions Default = new SerializationOptions(false);

        readonly bool roundtrip;
        readonly Type[] serializers;

        public SerializationOptions(bool roundtrip, params Type[] serializers)
        {
            this.roundtrip = roundtrip;
            this.serializers = serializers ?? new Type[0];
        }

        internal void Setup() => 
            SerializationManager.InitializeForTesting(
                serializers.Select(x => x.GetTypeInfo()).ToList());

        internal object Roundtrip(object obj) => roundtrip
            ? SerializationManager.RoundTripSerializationForTesting(obj)
            : obj;
    }
}