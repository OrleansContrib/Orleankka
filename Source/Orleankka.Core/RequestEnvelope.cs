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
        public readonly string Target;
        public readonly object Message;

        internal RequestEnvelope(string target, object message)
        {
            Target = target;
            Message = message;
        }

        [SerializerMethod]
        internal static void Serialize(object obj, BinaryTokenStreamWriter stream, Type expected)
        {
            var envelope = (RequestEnvelope) obj;
            stream.Write(envelope.Target);

            var bytes = MessageEnvelope.Serializer.Serialize(envelope.Message);
            SerializationManager.SerializeInner(bytes, stream, typeof(byte[]));
        }

        [DeserializerMethod]
        internal static object Deserialize(Type t, BinaryTokenStreamReader stream)
        {
            var target = stream.ReadString();
            
            var bytes = (byte[])SerializationManager.DeserializeInner(typeof(byte[]), stream);
            var message = MessageEnvelope.Serializer.Deserialize(bytes);

            return new RequestEnvelope(target, message);
        }

        [CopierMethod]
        internal static object DeepCopy(object original)
        {
            return original;
        }
    }
}
