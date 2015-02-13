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
        public readonly object Message;

        internal ResponseEnvelope(object message)
        {
            Message = message;
        }

        [SerializerMethod]
        internal static void Serialize(object obj, BinaryTokenStreamWriter stream, Type expected)
        {
            var envelope = (ResponseEnvelope)obj;

            var bytes = (envelope.Message == null)
                        ? new byte[0] 
                        : MessageEnvelope.Serializer.Serialize(envelope.Message);

            SerializationManager.SerializeInner(bytes, stream, typeof(byte[]));
        }

        [DeserializerMethod]
        internal static object Deserialize(Type t, BinaryTokenStreamReader stream)
        {
            var bytes = (byte[])SerializationManager.DeserializeInner(typeof(byte[]), stream);
            
            var message = (bytes.Length == 0)
                          ? null
                          : MessageEnvelope.Serializer.Deserialize(bytes);

            return new ResponseEnvelope(message);
        }

        [CopierMethod]
        internal static object DeepCopy(object original)
        {
            return original;
        }
    }
}
