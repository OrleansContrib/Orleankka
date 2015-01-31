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
            SerializationManager.SerializeInner(MessageEnvelope.Serializer(envelope.Message), stream, typeof(byte[]));
        }

        [DeserializerMethod]
        internal static object Deserialize(Type t, BinaryTokenStreamReader stream)
        {
            var message = MessageEnvelope.Deserializer((byte[])SerializationManager.DeserializeInner(typeof(byte[]), stream));
            return new ResponseEnvelope(message);
        }

        [CopierMethod]
        internal static object DeepCopy(object original)
        {
            return original;
        }
    }
}
