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
        internal static void Serialize(object obj, BinaryTokenStreamWriter stream, Type unused)
        {
            var envelope = (ResponseEnvelope)obj;

            if (envelope.Result == null)
            {
                stream.Write(ResultToken.Null);
                return;
            }

            stream.Write(ResultToken.Some);
            MessageEnvelope.Serializer.Serialize(envelope.Result, stream);
        }

        [DeserializerMethod]
        internal static object Deserialize(Type unused, BinaryTokenStreamReader stream)
        {
            byte resultToken = stream.ReadByte();

            if (resultToken == ResultToken.Null)
                return new ResponseEnvelope(null);

            if (resultToken != ResultToken.Some)
                throw new NotSupportedException();

            var result = MessageEnvelope.Serializer.Deserialize(stream);
            return new ResponseEnvelope(result);
        }

        [CopierMethod]
        internal static object DeepCopy(object original)
        {
            return original;
        }

        static class ResultToken
        {
            public const byte Null = 0;
            public const byte Some = 1;
        }
    }
}
