using System;
using System.Linq;

using Orleans.CodeGeneration;
using Orleans.Serialization;

namespace Orleankka.Core
{
    /// <summary> 
    /// FOR INTERNAL USE ONLY! 
    /// </summary>
    public class ResponseEnvelope
    {
        public readonly object Result;

        internal ResponseEnvelope(object result)
        {
            Result = result;
        }

        [SerializerMethod]
        internal static void Serialize(object obj, BinaryTokenStreamWriter stream, Type expected)
        {
            var envelope = (ResponseEnvelope)obj;

            var bytes = envelope.Result != null
                        ? MessageEnvelope.Serializer.Serialize(envelope.Result) 
                        : new byte[0];

            SerializationManager.SerializeInner(bytes, stream, typeof(byte[]));
        }

        [DeserializerMethod]
        internal static object Deserialize(Type t, BinaryTokenStreamReader stream)
        {
            var bytes = (byte[])SerializationManager.DeserializeInner(typeof(byte[]), stream);
            
            var message = bytes.Length != 0
                          ? MessageEnvelope.Serializer.Deserialize(bytes)
                          : null;

            return new ResponseEnvelope(message);
        }

        [CopierMethod]
        internal static object DeepCopy(object original)
        {
            return original;
        }
    }
}
