using System;
using System.Collections.Concurrent;
using System.Reflection;

using Google.Protobuf;
using Orleans.Serialization;

namespace Example
{
    public class ProtobufSerializer : IExternalSerializer
    {
        static readonly ConcurrentDictionary<RuntimeTypeHandle, MessageParser> Parsers = new ConcurrentDictionary<RuntimeTypeHandle, MessageParser>();

        public bool IsSupportedType(Type itemType)
        {
            if (!typeof(IMessage).IsAssignableFrom(itemType))
                return false;

            if (Parsers.ContainsKey(itemType.TypeHandle))
                return true;

            var prop = itemType.GetProperty("Parser", BindingFlags.Public | BindingFlags.Static);
            if (prop == null)
                return false;

            var parser = prop.GetValue(null, null);
            Parsers.TryAdd(itemType.TypeHandle, parser as MessageParser);

            return true;
        }

        public object DeepCopy(object source, ICopyContext context)
        {
            if (source == null)
                return null;

            dynamic dynamicSource = source;
            return dynamicSource.Clone();
        }

        public void Serialize(object item, ISerializationContext context, Type expectedType)
        {
            var writer = context.StreamWriter;

            if (item == null)
            {
                // Special handling for null value. 
                // Since in this ProtobufSerializer we are usually writing the data lengh as 4 bytes
                // we also have to write the Null object as 4 bytes lengh of zero.
                writer.Write(0);
                return;
            }

            if (!(item is IMessage iMessage))
                throw new ArgumentException("The provided item for serialization in not an instance of " + typeof(IMessage), nameof(item));

            var outBytes = iMessage.ToByteArray();
            writer.Write(outBytes.Length);
            writer.Write(outBytes);
        }

        public object Deserialize(Type expectedType, IDeserializationContext context)
        {
            var typeHandle = expectedType.TypeHandle;

            if (!Parsers.TryGetValue(typeHandle, out var parser))
                throw new ArgumentException("No parser found for the expected type " + expectedType, nameof(expectedType));

            var reader = context.StreamReader;

            var length = reader.ReadInt();
            var data = reader.ReadBytes(length);

            return parser.ParseFrom(data);
        }
    }
}