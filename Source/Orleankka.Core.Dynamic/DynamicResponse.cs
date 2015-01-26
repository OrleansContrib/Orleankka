using System;
using System.Linq;

using Orleans.CodeGeneration;
using Orleans.Serialization;

namespace Orleankka.Dynamic.Internal
{
    /// <summary> 
    /// FOR INTERNAL USE ONLY! 
    /// </summary>
    public class DynamicResponse
    {
        public readonly object Message;

        internal DynamicResponse(object message)
        {
            Message = message;
        }

        [SerializerMethod]
        internal static void Serialize(object obj, BinaryTokenStreamWriter stream, Type expected)
        {
            var response = (DynamicResponse)obj;
            SerializationManager.SerializeInner(DynamicMessage.Serializer(response.Message), stream, typeof(byte[]));
        }

        [DeserializerMethod]
        internal static object Deserialize(Type t, BinaryTokenStreamReader stream)
        {
            var message = DynamicMessage.Deserializer((byte[])SerializationManager.DeserializeInner(typeof(byte[]), stream));
            return new DynamicResponse(message);
        }

        [CopierMethod]
        internal static object DeepCopy(object original)
        {
            return original;
        }
    }
}
