using System;
using System.Linq;

using Orleans.CodeGeneration;
using Orleans.Serialization;

namespace Orleankka.Internal
{
    /// <summary> 
    /// FOR INTERNAL USE ONLY! 
    /// </summary>
    public class Response
    {
        public readonly object Message;

        internal Response(object message)
        {
            Message = message;
        }

        [SerializerMethod]
        internal static void Serialize(object obj, BinaryTokenStreamWriter stream, Type expected)
        {
            var response = (Response)obj;
            SerializationManager.SerializeInner(Internal.Payload.Serialize(response.Message), stream, typeof(byte[]));
        }

        [DeserializerMethod]
        internal static object Deserialize(Type t, BinaryTokenStreamReader stream)
        {
            var message = Internal.Payload.Deserialize((byte[])SerializationManager.DeserializeInner(typeof(byte[]), stream));
            return new Response(message);
        }

        [CopierMethod]
        internal static object DeepCopy(object original)
        {
            return original;
        }
    }
}
