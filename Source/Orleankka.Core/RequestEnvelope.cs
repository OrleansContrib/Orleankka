using System;
using System.Linq;

using Orleans.CodeGeneration;
using Orleans.Serialization;

namespace Orleankka.Core
{
    /// <summary> 
    /// FOR INTERNAL USE ONLY! 
    /// </summary>
    public class RequestEnvelope
    {
        public readonly ActorPath Target;
        public readonly object Message;

        internal RequestEnvelope(ActorPath target, object message)
        {
            Target = target;
            Message = message;
        }

        [SerializerMethod]
        internal static void Serialize(object obj, BinaryTokenStreamWriter stream, Type expected)
        {
            var envelope = (RequestEnvelope) obj;
            SerializationManager.SerializeInner(envelope.Target, stream, typeof(ActorPath));
            SerializationManager.SerializeInner(MessageEnvelope.Serializer(envelope.Message), stream, typeof(byte[]));
        }

        [DeserializerMethod]
        internal static object Deserialize(Type t, BinaryTokenStreamReader stream)
        {
            var target  = (ActorPath)SerializationManager.DeserializeInner(typeof(ActorPath), stream);
            var message = MessageEnvelope.Deserializer((byte[])SerializationManager.DeserializeInner(typeof(byte[]), stream));
            return new RequestEnvelope(target, message);
        }

        [CopierMethod]
        internal static object DeepCopy(object original)
        {
            return original;
        }
    }
}
