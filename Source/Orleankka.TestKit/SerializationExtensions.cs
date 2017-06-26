using Orleans.Serialization;

namespace Orleankka.TestKit
{
    static class SerializationExtensions
    {
        internal static object Roundtrip(this SerializationManager manager, object obj) => 
            manager != null 
            ? manager.RoundTripSerializationForTesting(obj) 
            : obj;
    }
}